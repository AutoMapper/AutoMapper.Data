using AutoMapper.Data.Configuration.Conventions;
using AutoMapper.Data.Mappers;

namespace AutoMapper.Data
{
    public static class ConfigurationExtensions
    {
        public static void AddDataReaderMapping(this IMapperConfigurationExpression configuration)
            => configuration.AddDataReaderMapping(false);

        public static void AddDataReaderMapping(this IMapperConfigurationExpression configuration, bool enableYieldReturn)
        {
            configuration.Mappers.Insert(0, new DataReaderMapper { YieldReturnEnabled = enableYieldReturn });
            configuration.AddMemberConfiguration().AddMember<DataRecordMemberConfiguration>();
        }
    }
}