<img src="https://s3.amazonaws.com/automapper/logo.png" alt="AutoMapper">
================================

#### The data extensions to AutoMapper, IDataReader support

##### Install via initialization:

```csharp
Mapper.Initialize(cfg => {
   cfg.AddDataReaderMapping();
   // Other config
});
```

##### Using `Profile`:
There are several ways to configure mapping with an instance of `Profile`:
1. Call `AddMemberConfiguration().AddMember<DataRecordMemberConfiguration>()` on the instance.
2. Call the `IMapperConfigurationExpression.AddDataReaderProfile` extension method.
3. Create an instance of Profile, call the `Profile.AddDataRecordMember` extension method on it, and add it to the configuration.
