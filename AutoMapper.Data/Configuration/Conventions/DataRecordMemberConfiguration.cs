using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using AutoMapper.Configuration.Conventions;
using AutoMapper.Data.Utils;
using AutoMapper.Internal;
using AutoMapper.Utils;

using static System.Reflection.Emit.OpCodes;

using TypeExtensions = AutoMapper.Utils.TypeExtensions;

namespace AutoMapper.Data.Configuration.Conventions
{
    public class DataRecordMemberConfiguration : ISourceToDestinationNameMapper
    {
        public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            nameToSearch = TransformNameFromSourceNamingConvention(sourceTypeDetails, nameToSearch);
            
            if (TypeExtensions.IsAssignableFrom(typeof(IDataRecord), sourceTypeDetails.Type))
            {
                var returnType = destMemberType;
                // TODO: The return type really should be the type of the field in the reader.
                // TODO: Remove the previous comment. We're going to add value conversion functionality, so using the destination
                //       type here can be done safely.
                var method = new DynamicMethod($"Get{nameToSearch.Replace(".", string.Empty) }", returnType, new[] { typeof(IDataRecord) }, true);
                var il = method.GetILGenerator();

                EmitPropertyMapping(il, destType, destMemberType, nameToSearch);
                il.Emit(Ret);

                return method;
            }
            return null;
        }

        public void Merge(ISourceToDestinationNameMapper otherNamedMapper)
        {
        }

        private void EmitPropertyMapping(ILGenerator il, Type destType, Type destMemberType, string nameToSearch)
        {
            var returnType = destMemberType;
            var fieldValueLocal = il.DeclareLocal(typeof(object));
            var returnValueLocal = il.DeclareLocal(returnType);
            var tryGetValueMethod = DataReaderHelper.TryGetValueMethod;
            var dbNullElseLabel = il.DefineLabel();
            var dbNullLabel = il.DefineLabel();
            var endIfDbNullLabel = il.DefineLabel();
            var getDbNullValueField = DataReaderHelper.DbNullValueField;
            var underlyingType = Nullable.GetUnderlyingType(destMemberType);
            var isNullable = (underlyingType != null);

            // Call TryGetValue
            il.Emit(Ldarg_0);
            il.Emit(Ldstr, nameToSearch);
            il.Emit(Ldloca_S, fieldValueLocal);
            il.Emit(Call, tryGetValueMethod);

            if (IsNestedObject(destMemberType))
            {
                var ctor = destMemberType.GetConstructor(Type.EmptyTypes);

                if (ctor != null)
                {
                    MethodInfo setMethod;
                    var objValueLocal = il.DeclareLocal(destMemberType);
                    var refersToMethod = DataReaderHelper.RefersToMethod;
                    var elseRefersToLabel = il.DefineLabel();
                    var endIfRefersToLabel = il.DefineLabel();

                    // If there are non-null fields referring to the nested property... 
                    il.Emit(Ldarg_0);
                    il.Emit(Ldstr, nameToSearch);
                    il.Emit(Call, refersToMethod);
                    il.Emit(Brfalse, elseRefersToLabel);
                    il.Emit(Newobj, ctor);
                    il.Emit(Stloc, objValueLocal);

                    foreach (PropertyInfo property in TypeExtensions.GetProperties(destMemberType))
                    {
                        if (property.CanWrite)
                        {
                            setMethod = TypeExtensions.GetSetMethod(property, true);
                            il.Emit(Ldloc, objValueLocal);
                            EmitPropertyMapping(il, destMemberType, property.PropertyType, $"{nameToSearch}.{property.Name}");
                            il.Emit(Callvirt, setMethod);
                        }
                    }
                    il.Emit(Br, endIfRefersToLabel);

                    // else do this...
                    il.MarkLabel(elseRefersToLabel);
                    il.Emit(Ldnull);
                    il.Emit(Stloc, objValueLocal);
                    il.MarkLabel(endIfRefersToLabel);

                    il.Emit(Ldloc, objValueLocal);
                    il.Emit(Stloc, fieldValueLocal);
                }
            }

            il.Emit(Pop);

            // Test if field value == null
            il.Emit(Ldloc, fieldValueLocal);
            il.Emit(Ldnull);
            il.Emit(Beq_S, dbNullLabel);

            // Test if field value == DBNull (skipped when source field does not exist)
            il.Emit(Ldloc, fieldValueLocal);
            il.Emit(Ldsfld, getDbNullValueField);
            il.Emit(Bne_Un_S, dbNullElseLabel);

            // If so...
            il.MarkLabel(dbNullLabel);
            il.Emit(Ldloca_S, returnValueLocal);
            il.Emit(Initobj, destMemberType);
            il.Emit(Br_S, endIfDbNullLabel);

            // else
            il.MarkLabel(dbNullElseLabel);
            il.Emit(Ldloc, fieldValueLocal);
            if (isNullable)
            {
                var nullableCtor = destMemberType.GetConstructor(new[] { underlyingType });
                il.Emit(Unbox_Any, underlyingType);
                il.Emit(Newobj, nullableCtor);
            }
            else
            {
            il.Emit(Unbox_Any, destMemberType);
            }
            il.Emit(Stloc, returnValueLocal);

            // end if
            il.MarkLabel(endIfDbNullLabel);

            // return
            il.Emit(Ldloc, returnValueLocal);
        }

        private bool IsNestedObject(Type type)
        {
            Type resolvedtype = Nullable.GetUnderlyingType(type) ?? type;

            return !resolvedtype.IsPrimitive();
        }

        private string TransformNameFromSourceNamingConvention(TypeDetails sourceTypeDetails, string nameToSearch)
        {
            // Sadly, Automapper's INamingConvention doesn't implement method to transform the nameToSearch.
            // Added support for LowerUnderscoreNamingConvention only.
            
            var memberConfiguration = sourceTypeDetails.Config.MemberConfiguration;

            var sourceNamingConvention = memberConfiguration.SourceNamingConvention;

            var destinationNamingConvention = memberConfiguration.DestinationNamingConvention;
            
            if (sourceNamingConvention is LowerUnderscoreNamingConvention)
            {
                return TransformToLowerUnderscoreCase(nameToSearch, destinationNamingConvention);
            }
            
            return nameToSearch;
        }

        private string TransformToLowerUnderscoreCase(string input, INamingConvention destinationNamingConvention)
        {
            var splitInput = destinationNamingConvention.Split(input);

            if (splitInput.Length == 0)
            {
                return input.ToLower();
            }
            
            return string.Join('_', splitInput.Select(x => x.ToLower()));
        }
    }
}
