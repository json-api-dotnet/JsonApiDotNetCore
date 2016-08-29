using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Abstractions;

namespace JsonApiDotNetCore.Configuration
{
  public interface IJsonApiModelConfiguration
  {
    /// <summary>
    /// The database context to use
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentException"></exception>
    void UseContext<T>();

    /// <summary>
    /// The request namespace.
    /// </summary>
    /// <param name="ns"></param>
    /// <example>api/v1</example>
    void SetDefaultNamespace(string ns);

    /// <summary>
    /// Define explicit mapping of a model to a class that implements IJsonApiResource
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="resourceType"></param>
    /// <exception cref="ArgumentException"></exception>
    void AddResourceMapping(Type modelType, Type resourceType);

    /// <summary>
    /// Specifies a controller override class for a particular model type.
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="controllerType"></param>
    /// <exception cref="ArgumentException"></exception>
    void UseController(Type modelType, Type controllerType);
  }
}
