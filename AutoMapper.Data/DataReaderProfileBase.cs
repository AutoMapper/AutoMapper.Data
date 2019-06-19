using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Data.Configuration.Conventions;
using AutoMapper.Mappers;

namespace AutoMapper.Data
{
    public abstract class DataReaderProfileBase : Profile
    {
        public DataReaderProfileBase()
        {
            AddMemberConfiguration().AddMember<DataRecordMemberConfiguration>();
        }
    }
}
