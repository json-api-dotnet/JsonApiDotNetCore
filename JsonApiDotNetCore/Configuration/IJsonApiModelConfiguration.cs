using AutoMapper;

namespace JsonApiDotNetCore.Configuration
{
  public interface IJsonApiModelConfiguration
  {
    void UseContext<T>();
    void SetDefaultNamespace(string ns);
    void DefineResourceMapping(MapperConfiguration mapperConfiguration);
  }
}
