![AutoMapper](https://s3.amazonaws.com/automapper/logo.png)
================================

#### The data extensions to AutoMapper, IDataReader support

[![CI](https://github.com/automapper/automapper.data/workflows/CI/badge.svg)](https://github.com/AutoMapper/AutoMapper.Data/actions?query=workflow%3ACI)
[![NuGet](http://img.shields.io/nuget/v/AutoMapper.Data.svg?label=NuGet)](https://www.nuget.org/packages/AutoMapper.Data/)
[![MyGet (dev)](https://img.shields.io/myget/automapperdev/vpre/AutoMapper.Data.svg?label=MyGet)](https://myget.org/feed/automapperdev/package/nuget/AutoMapper.Data)

##### Install via initialization:

```csharp
var mapper = new Mapper(cfg => {
   cfg.AddDataReaderMapping();
   cfg.CreateMap<IDataRecord, MyDto>();
   cfg.CreateMap<IDataRecord, MyOtherDto>();
   // Other config
});

// or with the AutoMapper.Extensions.Microsoft.DependencyInjection package:

services.AddAutoMapper(typeof(Startup), cfg => {
	cfg.AddDataReaderMapping();
});
```

You will need to configure maps for each `IDataRecord` DTO mapping.

##### Using `Profile`:

There are several ways to configure mapping with an instance of `Profile`:

- Create an instance of Profile, call the `Profile.AddDataRecordMember` extension method on it, and add it to the configuration.
- Call `AddMemberConfiguration().AddMember<DataRecordMemberConfiguration>()` on the instance.
- Call the `IMapperConfigurationExpression.AddDataReaderProfile` extension method.
