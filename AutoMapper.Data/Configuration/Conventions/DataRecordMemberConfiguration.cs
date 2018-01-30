using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Data.Utils;
using AutoMapper.Utils;
using static System.Reflection.Emit.OpCodes;
using TypeExtensions = AutoMapper.Utils.TypeExtensions;

namespace AutoMapper.Data.Configuration.Conventions
{
    public class DataRecordMemberConfiguration : IChildMemberConfiguration
    {
        private static readonly HashSet<Type> _treatAsPrimitives = new HashSet<Type>(new[] { typeof(DateTime), typeof(decimal), typeof(Guid), typeof(string) });

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfiguration parent)
        {
            if (TypeExtensions.IsAssignableFrom(typeof(IDataRecord), sourceType.Type))
            {
                var returnType = destMemberType;
                // TODO: The return type really should be the type of the field in the reader.
                // TODO: Remove the previous comment. We're going to add value conversion functionality, so using the destination
                //       type here can be done safely.
                var method = new DynamicMethod($"Get{nameToSearch.Replace(".", string.Empty) }", returnType, new[] { typeof(IDataRecord) }, true);
                var il = method.GetILGenerator();

                EmitPropertyMapping(il, destType, destMemberType, nameToSearch);
                il.Emit(Ret);

                resolvers.AddLast(method);

                return true;
            }
            return false;
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

            // Call TryGetValue
            il.Emit(Ldarg_0);
            il.Emit(Ldstr, nameToSearch);
            il.Emit(Ldloca_S, fieldValueLocal);
            il.Emit(Call, tryGetValueMethod);

            if (IsNestedObject(destMemberType))
            {
                var ctor = destMemberType.GetConstructor(Type.EmptyTypes);
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
            il.Emit(Unbox_Any, destMemberType);
            il.Emit(Stloc, returnValueLocal);

            // end if
            il.MarkLabel(endIfDbNullLabel);

            // return
            il.Emit(Ldloc, returnValueLocal);
        }

        private bool IsNestedObject(Type type)
        {
            Type resolvedtype = Nullable.GetUnderlyingType(type) ?? type;

            return !(resolvedtype.IsPrimitive() || _treatAsPrimitives.Contains(resolvedtype) || resolvedtype.IsEnum());
        }
    }
}
