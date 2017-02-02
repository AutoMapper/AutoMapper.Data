using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Data.Utils
{
    public static class DataReaderHelper
    {
        public static IEnumerable<IDataRecord> AsEnumerable(IDataReader reader)
       {
            return new DataReaderEnumerableAdapter(reader);
        }

        public static IEnumerable<T> AsYieldReturn<T>(IDataReader reader, ResolutionContext context, Func<IDataReader, ResolutionContext, object> mapFunc)
        {            
            while(reader.Read())
            {
                yield return (T)mapFunc(reader, context);
            }
        }

        private static bool TryGetValue(IDataRecord dataRecord, string fieldName, out object value)
        {
            try
            {
                value = dataRecord[fieldName];
                return true;
            }
            catch (IndexOutOfRangeException) // This is what the docs say gets thrown
            {
                value = null;
            }
            catch (ArgumentException) // This is what actually gets thrown
            {
                value = null;
            }

            return false;
        }

        private static bool RefersTo(IDataRecord dataRecord, string fieldName)
        {
            int fieldCount = dataRecord.FieldCount;
            string currentFieldName;

            for (int ordinal = 0; ordinal < fieldCount; ++ordinal)
            {
                currentFieldName = dataRecord.GetName(ordinal);

                if (currentFieldName.Split('.').FirstOrDefault(s => s == fieldName) != null)
                {
                    if (dataRecord[ordinal] != DBNull.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Lazy<MethodInfo> _getTryGetValue = new Lazy<MethodInfo>(() => typeof(DataReaderHelper).GetMethod(nameof(DataReaderHelper.TryGetValue), BindingFlags.NonPublic | BindingFlags.Static), true);
        private static Lazy<MethodInfo> _getDataReaderAsEnumerable = new Lazy<MethodInfo>(() => typeof(DataReaderHelper).GetMethod(nameof(DataReaderHelper.AsEnumerable)), true);
        private static Lazy<MethodInfo> _getDataReaderAsYieldReturn = new Lazy<MethodInfo>(() => typeof(DataReaderHelper).GetMethod(nameof(DataReaderHelper.AsYieldReturn)), true);
        private static Lazy<FieldInfo> _getDbNullValue = new Lazy<FieldInfo>(() => typeof(DBNull).GetField(nameof(DBNull.Value)), true);
        private static Lazy<MethodInfo> _getRefersTo = new Lazy<MethodInfo>(() => typeof(DataReaderHelper).GetMethod(nameof(DataReaderHelper.RefersTo), BindingFlags.NonPublic | BindingFlags.Static), true);

        public static MethodInfo TryGetValueMethod => _getTryGetValue.Value;
        public static MethodInfo DataReaderAsEnumerableMethod => _getDataReaderAsEnumerable.Value;
        public static MethodInfo DataReaderAsYieldReturnMethod => _getDataReaderAsYieldReturn.Value;
        public static FieldInfo DbNullValueField => _getDbNullValue.Value;
        public static MethodInfo RefersToMethod => _getRefersTo.Value;
    }
}
