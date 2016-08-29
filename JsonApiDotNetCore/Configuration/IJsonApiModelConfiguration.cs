using System;
using System.Collections.Generic;
using AutoMapper;
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
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TResource"></typeparam>
    /// <param name="mappingExpression"></param>
    /// <exception cref="ArgumentException"></exception>
    void AddResourceMapping<TModel, TResource>(Action<IMappingExpression> mappingExpression);

    /// <summary>
    /// Specifies a controller override class for a particular model type.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TController"></typeparam>
    /// <exception cref="ArgumentException"></exception>
    void UseController<TModel, TController>();
  }
}
