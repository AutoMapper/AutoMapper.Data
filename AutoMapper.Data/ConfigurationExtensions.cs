using AutoMapper.Data.Configuration.Conventions;
using AutoMapper.Data.Mappers;
using AutoMapper.Internal;

namespace AutoMapper.Data
{
    public static class ConfigurationExtensions
    {
        public static void AddDataReaderProfile(this IMapperConfigurationExpression configuration, Profile profile)
        {
            configuration.AddDataReaderMapping();
            profile.AddDataRecordMember();
            configuration.AddProfile(profile);
        }

        public static void AddDataReaderMapping(this IMapperConfigurationExpression configuration)
            => configuration.AddDataReaderMapping(false);

        public static void AddDataReaderMapping(this IMapperConfigurationExpression configuration, bool enableYieldReturn)
        {
            configuration.Internal().Mappers.Insert(0, new DataReaderMapper { YieldReturnEnabled = enableYieldReturn });
            configuration.AddDataRecordMember();
        }

        public static void AddDataRecordMember(this IProfileExpression profile) => 
            profile.Internal().MemberConfiguration.NameToMemberMappers.Add(new DataRecordMemberConfiguration());
    }
}