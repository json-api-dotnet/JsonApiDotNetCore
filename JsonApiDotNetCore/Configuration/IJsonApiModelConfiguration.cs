using System;
using System.Collections.Generic;
using AutoMapper;

namespace JsonApiDotNetCore.Configuration
{
  public interface IJsonApiModelConfiguration
  {
    void UseContext<T>();
    void SetDefaultNamespace(string ns);
    void DefineResourceMapping(Action<Dictionary<Type,Type>> mapping);
  }
}
