namespace AutoMapper.Data.Tests
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using AutoMapper.Data.Configuration.Conventions;
    using Shouldly;
    using Xunit;

    public abstract class ProfileTestsBase
    {
        public ProfileTestsBase()
        {
            Mapper.Reset();
            Mapper.Initialize(ConfigureMapper);
            DataReader = new DataBuilder().BuildDataReader();
            Results = Mapper.Map<IDataReader, IEnumerable<DTOObject>>(DataReader);
            Result = Results.FirstOrDefault();
        }

        protected virtual void ConfigureMapper(IMapperConfigurationExpression cfg)
        {
            cfg.AddDataReaderMapping(false);
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
        public void Then_a_column_containing_a_boolean_should_be_read()
        {
            Result.Boolean.ShouldBe(DataReader[FieldName.Boolean]);
        }

        [Fact]
        public void Then_a_projected_column_should_be_read()
        {
            Result.Else.ShouldBe(DataReader.GetDateTime(10));
        }

        internal class DtoProfile : Profile
        {
            public DtoProfile()
            {
                CreateMap<IDataRecord, DTOObject>()
                    .ForMember(dest => dest.Else, options => options.MapFrom(src => src.GetDateTime(10)));
            }
        }

        protected DTOObject Result { get; set; }

        protected IEnumerable<DTOObject> Results { get; set; }

        protected IDataReader DataReader { get; set; }
    }

    public class When_using_mapper_config_extension_to_configure_mapping : ProfileTestsBase
    {
        protected override void ConfigureMapper(IMapperConfigurationExpression cfg)
        {
            base.ConfigureMapper(cfg);
            cfg.AddDataReaderProfile(new DtoProfile());
        }
    }

    public class When_using_a_profile_config_extension_to_configure_mapping : ProfileTestsBase
    {
        protected override void ConfigureMapper(IMapperConfigurationExpression cfg)
        {
            Profile profile = new DtoProfile();

            base.ConfigureMapper(cfg);
            profile.AddDataRecordMember();
            cfg.AddProfile(profile);
        }
    }

    public class When_using_a_DataReaderProfile_subclass_to_configure_mapping : ProfileTestsBase
    {
        protected override void ConfigureMapper(IMapperConfigurationExpression cfg)
        {
            base.ConfigureMapper(cfg);
            cfg.AddProfile<DataReaderProfile>();
        }

        internal class DataReaderProfile : DataReaderProfileBase
        {
            public DataReaderProfile()
            {
                CreateMap<IDataRecord, DTOObject>()
                    .ForMember(dest => dest.Else, options => options.MapFrom(src => src.GetDateTime(10)));
            }
        }
    }
}
