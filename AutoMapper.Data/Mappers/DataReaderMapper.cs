using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Data.Utils;
using AutoMapper.Mappers.Internal;
using AutoMapper.Utils;
using static System.Linq.Expressions.Expression;
using ExpressionExtensions = AutoMapper.Utils.ExpressionExtensions;
using TypeHelper = AutoMapper.Utils.TypeHelper;

namespace AutoMapper.Data.Mappers
{
    public class DataReaderMapper : IObjectMapper
    {
        public bool YieldReturnEnabled { get; set; }

        public bool IsMatch(TypePair context)
            => IsDataReader(context.SourceType, context.DestinationType);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            Expression mapExpr = null;
            
            if (IsDataReader(sourceExpression.Type, destExpression.Type))
            {
                ParameterExpression itemParam;
                Expression itemExpr;

                try
                {
                    itemExpr = CollectionMapperExpressionFactory.MapItemExpr(configurationProvider, profileMap, typeof(IEnumerable<IDataRecord>), destExpression.Type, contextExpression, out itemParam);
                }
                catch (Exception ex)
                {
                    throw new AutoMapperMappingException("Missing type map configuration or unsupported mapping.", ex, new TypePair(sourceExpression.Type, destExpression.Type));
                }

                if (YieldReturnEnabled)
                {
                    var mapFunc = Lambda(itemExpr, itemParam, contextExpression as ParameterExpression);
                    MethodInfo genericMapFunc = DataReaderHelper.DataReaderAsYieldReturnMethod.MakeGenericMethod(TypeHelper.GetElementType(destExpression.Type));
                    var sourceAsYieldReturn = Call(null, genericMapFunc, sourceExpression, contextExpression, mapFunc);

                    mapExpr =
                        Block(sourceAsYieldReturn);
                }
                else
                {
                    var sourceAsEnumerable = Call(null, DataReaderHelper.DataReaderAsEnumerableMethod, sourceExpression);
                    var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type)); // Cache this if we experience poor performance
                    var listVar = Variable(listType, "list");
                    var listAddExpr = Call(listVar, listType.GetMethod("Add"), itemExpr); // Cache this if we experience poor performance

                    mapExpr =
                        Block(new[] { listVar }, 
                            Assign(listVar, New(listType)), 
                            ExpressionExtensions.ForEach(sourceAsEnumerable, itemParam, listAddExpr), 
                            listVar);
                }
            }

            return mapExpr;
        }

        private static bool IsDataReader(Type sourceType, Type destinationType)
        {
            return typeof(IDataReader).GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo())
                && destinationType.IsEnumerableType();
        }
    }
}
