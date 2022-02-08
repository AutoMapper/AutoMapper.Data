using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using static System.Linq.Expressions.Expression;
using static AutoMapper.Utils.ExpressionExtensions;

namespace AutoMapper.Utils
{
    using AutoMapper.Internal;
    using AutoMapper.Internal.Mappers;
    using System.Collections.Generic;

    public static class CollectionMapperExtensions
    {
        internal static Expression MapCollectionExpression(this IGlobalConfiguration configurationProvider, 
           ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression,
           Expression destExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Variable(destExpression.Type, "passedDestination");
            var condition = conditionalExpression(passedDestination);
            var newExpression = Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            ParameterExpression itemParam;

            var itemExpr = mapItem(configurationProvider, profileMap, memberMap, sourceExpression.Type, passedDestination.Type, out itemParam);

            var destinationElementType = itemExpr.Type;
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");
            var destination = memberMap?.UseDestinationValue == true ? passedDestination : newExpression;
            var addItems = ForEach(sourceExpression, itemParam, Call(destination, addMethod, itemExpr));

            var mapExpr = Block(addItems, destination);

            var ifNullExpr = profileMap.AllowNullCollections ? Constant(null, passedDestination.Type) : (Expression) newExpression;
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var checkNull =  
                Block(new[] { newExpression, passedDestination },
                    Assign(passedDestination, destExpression),
                    IfThenElse(condition ?? Constant(false),
                                    Block(Assign(newExpression, passedDestination), Call(newExpression, clearMethod)),
                                    Assign(newExpression, passedDestination.Type.NewExpr(ifInterfaceType))),
                    Condition(Equal(sourceExpression, Constant(null)), ToType(ifNullExpr, passedDestination.Type), ToType(mapExpr, passedDestination.Type))
                );
            if(memberMap != null)
            {
                return checkNull;
            }
            var elementTypeMap = configurationProvider.ResolveTypeMap(sourceElementType, destinationElementType);
            if(elementTypeMap == null)
            {
                return checkNull;
            }
            var checkContext = ExpressionBuilder.CheckContext(elementTypeMap);
            if(checkContext == null)
            {
                return checkNull;
            }
            return Block(checkContext, checkNull);
        }

        internal static Delegate Constructor(Type type)
        {
            return Lambda(ToType(ObjectFactory.GenerateConstructorExpression(type), type)).Compile();
        }

        internal static Expression NewExpr(this Type baseType, Type ifInterfaceType)
        {
            var newExpr = baseType.IsInterface()
                ? New(ifInterfaceType.MakeGenericType(TypeHelper.GetElementTypes(baseType, ElementTypeFlags.BreakKeyValuePair)))
                : ObjectFactory.GenerateConstructorExpression(baseType);
            return ToType(newExpr, baseType);
        }

        public delegate Expression MapItem(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Type sourceType, Type destType, out ParameterExpression itemParam);

        internal static Expression MapItemExpr(this IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Type sourceType, Type destType, out ParameterExpression itemParam)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);
            itemParam = Parameter(sourceElementType, "item");

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, typePair, itemParam, memberMap);
            return ToType(itemExpr, destElementType);
        }

        internal static Expression MapKeyPairValueExpr(this IGlobalConfiguration configurationProvider,
            ProfileMap profileMap, MemberMap memberMap, Type sourceType, Type destType, out ParameterExpression itemParam)
        {
            var sourceElementTypes = TypeHelper.GetElementTypes(sourceType, ElementTypeFlags.BreakKeyValuePair);
            var destElementTypes = TypeHelper.GetElementTypes(destType, ElementTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = typeof(KeyValuePair<,>).MakeGenericType(sourceElementTypes);
            itemParam = Parameter(sourceElementType, "item");
            var destElementType = typeof(KeyValuePair<,>).MakeGenericType(destElementTypes);

            var keyExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, typePairKey, Property(itemParam, "Key"), memberMap);
            var valueExpr = ExpressionBuilder.MapExpression(configurationProvider, profileMap, typePairValue, Property(itemParam, "Value"), memberMap);
            var keyPair = New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }

        internal static BinaryExpression IfNotNull(Expression destExpression)
        {
            return NotEqual(destExpression, Constant(null));
        }
    }

    public class CollectionMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
            => configurationProvider.MapCollectionExpression(profileMap, memberMap, sourceExpression, destExpression, CollectionMapperExtensions.IfNotNull, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
    }
}