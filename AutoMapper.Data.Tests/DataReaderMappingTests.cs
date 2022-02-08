namespace AutoMapper.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using AutoMapper.Internal;
    using Configuration.Conventions;
    using Mappers;
    using Shouldly;
    using Xunit;

    public class When_mapping_a_data_reader_to_a_dto
    {
        public When_mapping_a_data_reader_to_a_dto()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping(YieldReturnEnabled);

                cfg.CreateMap<IDataRecord, DTOObject>()
                    .ForMember(dest => dest.Else, options => options.MapFrom(src => src.GetDateTime(10)));
            });

            Mapper = new Mapper(configuration);
            DataReader = new DataBuilder().BuildDataReader();
            Results = Mapper.Map<IDataReader, IEnumerable<DTOObject>>(DataReader);
            Result = Results.FirstOrDefault();
        }

        [Fact]
        public void Then_a_column_containing_a_small_integer_should_be_read()
        {
            Result.SmallInteger.ShouldBe(DataReader[FieldName.SmallInt]);
        }

        [Fact]
        public void Then_a_column_containing_an_integer_should_be_read()
        {
            Result.Integer.ShouldBe(DataReader[FieldName.Int]);
        }

        [Fact]
        public void Then_a_column_containing_a_big_integer_should_be_read()
        {
            Result.BigInteger.ShouldBe(DataReader[FieldName.BigInt]);
        }

        [Fact]
        public void Then_a_column_containing_a_GUID_should_be_read()
        {
            Result.Guid.ShouldBe(DataReader[FieldName.Guid]);
        }

        [Fact]
        public void Then_a_column_containing_a_float_should_be_read()
        {
            Result.Float.ShouldBe(DataReader[FieldName.Float]);
        }

        [Fact]
        public void Then_a_column_containing_a_double_should_be_read()
        {
            Result.Double.ShouldBe(DataReader[FieldName.Double]);
        }

        [Fact]
        public void Then_a_column_containing_a_decimal_should_be_read()
        {
            Result.Decimal.ShouldBe(DataReader[FieldName.Decimal]);
        }

        [Fact]
        public void Then_a_column_containing_a_date_and_time_should_be_read()
        {
            Result.DateTime.ShouldBe(DataReader[FieldName.DateTime]);
        }

        [Fact]
        public void Then_a_column_containing_a_byte_should_be_read()
        {
            Result.Byte.ShouldBe(DataReader[FieldName.Byte]);
        }

        [Fact]
        public void Then_a_column_containing_a_byte_array_should_be_read()
        {
            Result.ByteArray.ShouldBe(DataReader[FieldName.ByteArray]);
        }

        [Fact]
        public void Then_a_column_containing_a_boolean_should_be_read()
        {
            Result.Boolean.ShouldBe(DataReader[FieldName.Boolean]);
        }

        [Fact]
        public void Then_a_column_containing_a_datetimeoffset_should_be_read()
        {
            Result.DateTimeOffset.ShouldBe(DataReader[FieldName.DateTimeOffset]);
        }

        [Fact]
        public void Then_a_projected_column_should_be_read()
        {
            Result.Else.ShouldBe(DataReader.GetDateTime(10));
        }

        [Fact]
        public void Should_have_valid_mapping()
        {
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        protected virtual bool YieldReturnEnabled => false;
        protected DTOObject Result { get; set; }
        protected IEnumerable<DTOObject> Results { get; set; }
        protected IDataReader DataReader { get; set; }

        protected IMapper Mapper { get; }
    }

    public class When_mapping_a_data_reader_to_matching_dtos
    {
        public When_mapping_a_data_reader_to_matching_dtos()
        {
             MapperConfiguration configuration = new MapperConfiguration(cfg =>{
                 cfg.AddDataReaderMapping();

                 cfg.CreateMap<IDataRecord, DTOObject>()
                     .ForMember(dest => dest.Else, options => options.MapFrom(src => src.GetDateTime(10)));
                 cfg.CreateMap<IDataRecord, DerivedDTOObject>()
                     .ForMember(dest => dest.Else, options => options.MapFrom(src => src.GetDateTime(10)));
             });

            _mapper = new Mapper(configuration);
            _mapper.Map<IDataReader, IEnumerable<DTOObject>>(new DataBuilder().BuildDataReader()).ToArray();
        }

        [Fact]
        public void Should_map_successfully()
        {
            var result = _mapper.Map<IDataReader, IEnumerable<DerivedDTOObject>>(new DataBuilder().BuildDataReader());
            result.Count().ShouldBe(1);
        }

        [Fact]
        public void Should_have_valid_mapping()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private IMapper _mapper;
    }
    /// <summary>
    /// The purpose of this test is to exercise the internal caching logic of DataReaderMapper.
    /// </summary>
    public class When_mapping_a_data_reader_to_a_dto_twice : When_mapping_a_data_reader_to_a_dto
    {
        public When_mapping_a_data_reader_to_a_dto_twice() 
        {
            DataReader = new DataBuilder().BuildDataReader();
            Results = Mapper.Map<IDataReader, IEnumerable<DTOObject>>(DataReader);
            Result = Results.FirstOrDefault();
        }
    }

    public class When_mapping_a_data_reader_using_the_default_configuration : When_mapping_a_data_reader_to_a_dto
    {
        [Fact]
        public void Then_the_enumerable_should_be_a_list()
        {
            Results.ShouldBeAssignableTo<IList<DTOObject>>();
        }
    }

    public class When_mapping_a_data_reader_using_the_yield_return_option : When_mapping_a_data_reader_to_a_dto
    {
        protected override bool YieldReturnEnabled => true;

        [Fact]
        public void Then_the_enumerable_should_not_be_a_list()
        {
            (Results is IList<DTOObject>).ShouldBeFalse();
        }
    }

    public class When_mapping_a_data_reader_to_a_dto_and_the_map_does_not_exist
    {
        public When_mapping_a_data_reader_to_a_dto_and_the_map_does_not_exist()
        {
            MapperConfiguration configuration = 
                new MapperConfiguration(cfg => cfg.Internal().Mappers.Insert(0, new DataReaderMapper()));

            _mapper = new Mapper(configuration);
            _dataReader = new DataBuilder().BuildDataReader();
        }

        [Fact]
        public void Then_an_automapper_exception_should_be_thrown()
        {
            var passed = false;
            try
            {
                _mapper.Map<IDataReader, IEnumerable<DTOObject>>(_dataReader).FirstOrDefault();
            }
            catch (AutoMapperMappingException)
            {
                passed = true;
            }

            passed.ShouldBeTrue();
        }

        private IDataReader _dataReader;
        private IMapper _mapper;
    }


    public class When_mapping_a_single_data_record_to_a_dto
    {
        public When_mapping_a_single_data_record_to_a_dto()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping();

                cfg.CreateMap<IDataRecord, DTOObject>()
                    .ForMember(dest => dest.Else, options => options.MapFrom(src => src.GetDateTime(src.GetOrdinal(FieldName.Something))));
            });

            _mapper = new Mapper(configuration);
            _dataRecord = new DataBuilder().BuildDataRecord();
            _result = _mapper.Map<IDataRecord, DTOObject>(_dataRecord);
        }

        [Fact]
        public void Then_a_column_containing_a_small_integer_should_be_read()
        {
            _result.SmallInteger.ShouldBe(_dataRecord[FieldName.SmallInt]);
        }

        [Fact]
        public void Then_a_column_containing_an_integer_should_be_read()
        {
            _result.Integer.ShouldBe(_dataRecord[FieldName.Int]);
        }

        [Fact]
        public void Then_a_column_containing_a_big_integer_should_be_read()
        {
            _result.BigInteger.ShouldBe(_dataRecord[FieldName.BigInt]);
        }

        [Fact]
        public void Then_a_column_containing_a_GUID_should_be_read()
        {
            _result.Guid.ShouldBe(_dataRecord[FieldName.Guid]);
        }

        [Fact]
        public void Then_a_column_containing_a_float_should_be_read()
        {
            _result.Float.ShouldBe(_dataRecord[FieldName.Float]);
        }

        [Fact]
        public void Then_a_column_containing_a_double_should_be_read()
        {
            _result.Double.ShouldBe(_dataRecord[FieldName.Double]);
        }

        [Fact]
        public void Then_a_column_containing_a_decimal_should_be_read()
        {
            _result.Decimal.ShouldBe(_dataRecord[FieldName.Decimal]);
        }

        [Fact]
        public void Then_a_column_containing_a_date_and_time_should_be_read()
        {
            _result.DateTime.ShouldBe(_dataRecord[FieldName.DateTime]);
        }

        [Fact]
        public void Then_a_column_containing_a_byte_should_be_read()
        {
            _result.Byte.ShouldBe(_dataRecord[FieldName.Byte]);
        }

        [Fact]
        public void Then_a_column_containing_a_boolean_should_be_read()
        {
            _result.Boolean.ShouldBe(_dataRecord[FieldName.Boolean]);
        }

        [Fact]
        public void Then_a_projected_column_should_be_read()
        {
            _result.Else.ShouldBe(_dataRecord[FieldName.Something]);
        }

        [Fact]
        public void Should_have_valid_mapping()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private DTOObject _result;
        private IDataRecord _dataRecord;
        private IMapper _mapper;
    }

    public class When_mapping_a_data_reader_to_a_dto_with_nullable_field
    {
        internal const string FieldName = "Integer";
        internal const int FieldValue = 7;

        internal class DtoWithSingleNullableField
        {
            public int? Integer { get; set; }
        }

        internal class DataBuilder
        {
            public IDataReader BuildDataReaderWithNullableField()
            {
                var table = new DataTable();

                var col = table.Columns.Add(FieldName, typeof(int));
                col.AllowDBNull = true;

                var row1 = table.NewRow();
                row1[FieldName] = FieldValue;
                table.Rows.Add(row1);

                var row2 = table.NewRow();
                row2[FieldName] = DBNull.Value;
                table.Rows.Add(row2);

                return table.CreateDataReader();
            }
        }

        public When_mapping_a_data_reader_to_a_dto_with_nullable_field()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping();

                cfg.CreateMap<IDataReader, DtoWithSingleNullableField>();
            });
            _mapper = new Mapper(configuration);

            _dataReader = new DataBuilder().BuildDataReaderWithNullableField();
        }

        [Fact]
        public void Then_results_should_be_as_expected()
        {
            while (_dataReader.Read())
            {
                var dto = _mapper.Map<IDataReader, DtoWithSingleNullableField>(_dataReader);

                if (_dataReader.IsDBNull(0))
                    dto.Integer.HasValue.ShouldBe(false);
                else
                {
                    // uncomment the following line to see some strange fail message that might be the key to the problem
                    dto.Integer.HasValue.ShouldBe(true);

                    dto.Integer.Value.ShouldBe(FieldValue);
                }
            }
        }

        [Fact]
        public void Should_have_valid_mapping()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private IDataReader _dataReader;
        private IMapper _mapper;
    }

    public class When_mapping_a_data_reader_to_a_dto_with_nullable_enum
    {
        internal const string FieldName = "Value";
        internal const int FieldValue = 3;

        public enum settlement_type
        {
            PreDelivery = 0,
            DVP = 1,
            FreeDelivery = 2,
            Prepayment = 3,
            Allocation = 4,
            SafeSettlement = 5,
        }
        internal class DtoWithSingleNullableField
        {
            public settlement_type? Value { get; set; }
        }

        internal class DataBuilder
        {
            public IDataReader BuildDataReaderWithNullableField()
            {
                var table = new DataTable();

                var col = table.Columns.Add(FieldName, typeof(int));
                col.AllowDBNull = true;

                var row1 = table.NewRow();
                row1[FieldName] = FieldValue;
                table.Rows.Add(row1);

                var row2 = table.NewRow();
                row2[FieldName] = DBNull.Value;
                table.Rows.Add(row2);

                return table.CreateDataReader();
            }
        }

        public When_mapping_a_data_reader_to_a_dto_with_nullable_enum()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping();

                cfg.CreateMap<IDataReader, DtoWithSingleNullableField>();
            });
            _mapper = new Mapper(configuration);

            _dataReader = new DataBuilder().BuildDataReaderWithNullableField();
        }

        [Fact]
        public void Then_results_should_be_as_expected()
        {
            while (_dataReader.Read())
            {
                var dto = _mapper.Map<IDataReader, DtoWithSingleNullableField>(_dataReader);

                object value = _dataReader[0];
                if (!Equals(value, DBNull.Value))
                    dto.Value = (settlement_type)value;

                if (_dataReader.IsDBNull(0))
                    dto.Value.HasValue.ShouldBeFalse();
                else
                {
                    dto.Value.HasValue.ShouldBeTrue();

                    dto.Value.Value.ShouldBe(settlement_type.Prepayment);
                }
            }
        }

        [Fact]
        public void Should_have_valid_mapping()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private IDataReader _dataReader;
        private IMapper _mapper { get; }
    }

    public class When_mapping_a_data_reader_to_a_dto_with_nested_dto
    {
        internal const string FieldName = "Integer";
        internal const int FieldValue = 7;
        internal const string InnerFieldName = "Inner.Descr";
        internal const string InnerFieldName2 = "Inner.Descr2";
        internal const string Inner2FieldName2 = "Inner2.Descr2";
        internal const string InnerFieldValue = "Hello";
        internal const string InnerFieldValue2 = "World";
        internal const string Inner2FieldValue2 = "2World2";

        internal class DtoInnerClass
        {
            public string Descr { get; set; }
            public string Descr2 { get; set; }
        }

        internal class DtoWithNestedClass
        {
            public int Integer { get; set; }
            public DtoInnerClass Inner { get; set; }
            public DtoInnerClass Inner2 { get; set; }
            public DtoInnerClass Inner3 { get; set; }
        }

        internal class DataBuilder
        {
            public IDataReader BuildDataReaderWithNestedClass()
            {
                var table = new DataTable();

                var col = table.Columns.Add(FieldName, typeof(int));
                col.AllowDBNull = true;
                table.Columns.Add(InnerFieldName, typeof(string));
                table.Columns.Add(InnerFieldName2, typeof(string));
                table.Columns.Add("Inner2.Descr", typeof(string));
                table.Columns.Add(Inner2FieldName2, typeof(string));
                table.Columns.Add("Inner3.Descr", typeof(string));
                table.Columns.Add("Inner3.Descr2", typeof(string));

                var row1 = table.NewRow();
                row1[FieldName] = FieldValue;
                row1[InnerFieldName] = InnerFieldValue;
                row1[InnerFieldName2] = InnerFieldValue2;
                row1["Inner2.Descr"] = null;
                row1[Inner2FieldName2] = Inner2FieldValue2;
                row1["Inner3.Descr"] = null;
                row1["Inner3.Descr2"] = null;
                table.Rows.Add(row1);

                return table.CreateDataReader();
            }
        }

        public When_mapping_a_data_reader_to_a_dto_with_nested_dto()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping();

                cfg.CreateMap<IDataRecord, DtoWithNestedClass>();
            });

            _mapper = new Mapper(configuration);
            _dataReader = new DataBuilder().BuildDataReaderWithNestedClass();
        }

        [Fact]
        public void Then_results_should_be_as_expected()
        {
            while (_dataReader.Read())
            {
                var dto = _mapper.Map<IDataReader, DtoWithNestedClass>(_dataReader);

                dto.Integer.ShouldBe(FieldValue);

                // nested property
                dto.Inner.ShouldNotBeNull();
                dto.Inner.Descr.ShouldBe(InnerFieldValue);
                dto.Inner.Descr2.ShouldBe(InnerFieldValue2);

                // more than one property
                dto.Inner2.ShouldNotBeNull();
                dto.Inner2.Descr.ShouldBeNull(); // null
                dto.Inner2.Descr2.ShouldBe(Inner2FieldValue2);

                // no Inner3 properties are set so the Inner3 property is null
                dto.Inner3.ShouldBeNull();
            }
        }

        private IDataReader _dataReader;
        private IMapper _mapper { get; }
    }

    public class When_mapping_a_data_reader_to_a_dto_with_missing_columns_in_data_reader
    {
        internal const string FieldName = "Integer";
        internal const int FieldValue = 7;

        internal class DtoWithMoreColumnsThanDataReader
        {
            public int Integer { get; set; }
            public int Integer2 { get; set; }
            public string String1 { get; set; }
        }

        internal class DataBuilder
        {
            public IDataReader BuildDataReaderWithMissingColumns()
            {
                var table = new DataTable();

                var col = table.Columns.Add(FieldName, typeof(int));
                col.AllowDBNull = true;

                var row1 = table.NewRow();
                row1[FieldName] = FieldValue;
                table.Rows.Add(row1);

                return table.CreateDataReader();
            }
        }

        public When_mapping_a_data_reader_to_a_dto_with_missing_columns_in_data_reader()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping();

                cfg.CreateMap<IDataRecord, DtoWithMoreColumnsThanDataReader>();
            });

            _mapper = new Mapper(configuration);
            _dataReader = new DataBuilder().BuildDataReaderWithMissingColumns();
        }

        [Fact]
        public void Then_results_should_be_as_expected()
        {
            while (_dataReader.Read())
            {
                var dto = _mapper.Map<IDataReader, DtoWithMoreColumnsThanDataReader>(_dataReader);

                dto.Integer.ShouldBe(FieldValue);

                // missing column
                dto.Integer2.ShouldBe(0);
                dto.String1.ShouldBeNull();
            }
        }

        private IDataReader _dataReader;
        private IMapper _mapper;
    }

   public class When_mapping_a_data_reader_to_a_dto_with_an_interface_member
    {
        private const string FieldName = "Name";
        private const string FieldValue = "Value";

        private static class DataBuilder
        {
            public static IDataReader BuildDataReader()
            {
                var table = new DataTable();
                var column = table.Columns.Add(FieldName, typeof(string));
                var row = table.NewRow();

                row[FieldName] = FieldValue;
                table.Rows.Add(row);

                return table.CreateDataReader();
            }
        }

        public interface INoneSuch
        {
            string Nop();
        }

        public class NoneSuch
        {
            public string Name { get; set; }
            public INoneSuch Value { get; set; }
        }

        [Fact]
        void Then_results_should_be_as_expected()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddDataReaderMapping();
                cfg.CreateMap<IDataRecord, NoneSuch>();
            });

            var mapper = new Mapper(configuration);
            var dataReader = DataBuilder.BuildDataReader();

            dataReader.Read();
            var result = mapper.Map<IDataReader, NoneSuch>(dataReader);

            result.Name.ShouldBe(dataReader[FieldName]);
            result.Value.ShouldBeNull();
        }
    }

    internal class FieldName
    {
        public const String SmallInt = "SmallInteger";
        public const String Int = "Integer";
        public const String BigInt = "BigInteger";
        public const String Guid = "Guid";
        public const String Float = "Float";
        public const String Double = "Double";
        public const String Decimal = "Decimal";
        public const String DateTime = "DateTime";
        public const String Byte = "Byte";
        public const String ByteArray = "ByteArray";
        public const String Boolean = "Boolean";
        public const String Something = "Something";
        public const String DateTimeOffset = "DateTimeOffset";
    }

    public class DataBuilder
    {
        public IDataReader BuildDataReader()
        {
            var authorizationSetDataTable = new DataTable();
            authorizationSetDataTable.Columns.Add(FieldName.SmallInt, typeof(Int16));
            authorizationSetDataTable.Columns.Add(FieldName.Int, typeof(Int32));
            authorizationSetDataTable.Columns.Add(FieldName.BigInt, typeof(Int64));
            authorizationSetDataTable.Columns.Add(FieldName.Guid, typeof(Guid));
            authorizationSetDataTable.Columns.Add(FieldName.Float, typeof(float));
            authorizationSetDataTable.Columns.Add(FieldName.Double, typeof(Double));
            authorizationSetDataTable.Columns.Add(FieldName.Decimal, typeof(Decimal));
            authorizationSetDataTable.Columns.Add(FieldName.DateTime, typeof(DateTime));
            authorizationSetDataTable.Columns.Add(FieldName.Byte, typeof(Byte));
            authorizationSetDataTable.Columns.Add(FieldName.Boolean, typeof(Boolean));
            authorizationSetDataTable.Columns.Add(FieldName.Something, typeof(DateTime));
            authorizationSetDataTable.Columns.Add(FieldName.ByteArray, typeof(Byte[]));
            authorizationSetDataTable.Columns.Add(FieldName.DateTimeOffset, typeof(DateTimeOffset));

            var authorizationSetDataRow = authorizationSetDataTable.NewRow();
            authorizationSetDataRow[FieldName.SmallInt] = 22;
            authorizationSetDataRow[FieldName.Int] = 6134;
            authorizationSetDataRow[FieldName.BigInt] = 61346154;
            authorizationSetDataRow[FieldName.Guid] = Guid.NewGuid();
            authorizationSetDataRow[FieldName.Float] = 642.61;
            authorizationSetDataRow[FieldName.Double] = 67164.64;
            authorizationSetDataRow[FieldName.Decimal] = 94341.61;
            authorizationSetDataRow[FieldName.DateTime] = DateTime.Now;
            authorizationSetDataRow[FieldName.Byte] = 0x12;
            authorizationSetDataRow[FieldName.Boolean] = true;
            authorizationSetDataRow[FieldName.Something] = DateTime.MaxValue;
            authorizationSetDataRow[FieldName.ByteArray] = new Byte[] { 0x01, 0x02, 0x03, 0x04 };
            authorizationSetDataRow[FieldName.DateTimeOffset] = new DateTimeOffset(2000, 7, 4, 6, 31, 45, new TimeSpan(-5, 30, 0));
            authorizationSetDataTable.Rows.Add(authorizationSetDataRow);

            return authorizationSetDataTable.CreateDataReader();
        }

        public IDataRecord BuildDataRecord()
        {
            var dataReader = BuildDataReader();
            dataReader.Read();
            return dataReader;
        }
    }

    public class DTOObject
    {
        public Int16 SmallInteger { get; private set; }
        public Int32 Integer { get; private set; }
        public Int64 BigInteger { get; private set; }
        public Guid Guid { get; private set; }
        public float Float { get; private set; }
        public Double Double { get; private set; }
        public Decimal Decimal { get; private set; }
        public DateTime DateTime { get; private set; }
        public Byte Byte { get; private set; }
        public Boolean Boolean { get; private set; }
        public DateTime Else { get; private set; }
        public Byte[] ByteArray { get; private set; }
        public DateTimeOffset DateTimeOffset { get; private set; }
    }

    public class DerivedDTOObject : DTOObject { }
}
