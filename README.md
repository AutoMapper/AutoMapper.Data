<img src="https://s3.amazonaws.com/automapper/logo.png" alt="AutoMapper">
================================

The data extensions to AutoMapper, IDataReader support

Install via initialization:

```
Mapper.Initialize(cfg => {
   MapperRegistry.Mappers.Add(new DataReaderMapper { EnableYieldReturn = true });
   // Other config
});
```