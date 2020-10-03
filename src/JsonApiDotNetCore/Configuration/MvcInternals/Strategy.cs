// // Copyright (c) .NET Foundation. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// using System;
// using System.Collections;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq.Expressions;
// using System.Reflection;
// using Microsoft.AspNetCore.Mvc.ModelBinding;
// using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
//
// namespace JsonApiDotNetCore.Configuration
// {
//         internal class JsonApiDefaultComplexObjectValidationStrategy : IValidationStrategy
//     {
//         private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
//
//         /// <summary>
//         /// Gets an instance of <see cref="DefaultComplexObjectValidationStrategy"/>.
//         /// </summary>
//         public static readonly IValidationStrategy Instance = new JsonApiDefaultComplexObjectValidationStrategy();
//
//         private JsonApiDefaultComplexObjectValidationStrategy()
//         {
//         }
//
//         /// <inheritdoc />
//         public IEnumerator<ValidationEntry> GetChildren(
//             ModelMetadata metadata,
//             string key,
//             object model)
//         {
//             return new Enumerator(metadata.Properties, key, model);
//         }
//
//         private class Enumerator : IEnumerator<ValidationEntry>
//         {
//             private readonly string _key;
//             private readonly object _model;
//             private readonly ModelPropertyCollection _properties;
//
//             private ValidationEntry _entry;
//             private int _index;
//
//             public Enumerator(
//                 ModelPropertyCollection properties,
//                 string key,
//                 object model)
//             {
//                 _properties = properties;
//                 _key = key;
//                 _model = model;
//
//                 _index = -1;
//             }
//
//             public ValidationEntry Current => _entry;
//
//             object IEnumerator.Current => Current;
//
//             public bool MoveNext()
//             {
//                 _index++;
//                 if (_index >= _properties.Count)
//                 {
//                     return false;
//                 }
//
//                 var property = _properties[_index];
//                 var propertyName = property.BinderModelName ?? property.PropertyName;
//                 var key = ModelNames.CreatePropertyModelName(_key, propertyName);
//
//                 if (_model == null)
//                 {
//                     // Performance: Never create a delegate when container is null.
//                     _entry = new ValidationEntry(property, key, model: null);
//                 }
//                 else if (IsMono)
//                 {
//                     _entry = new ValidationEntry(property, key, () => GetModelOnMono(_model, property.PropertyName));
//                 }
//                 else
//                 {
//                     _entry = new ValidationEntry(property, key, () => GetModel(_model, property));
//                 }
//
//                 return true;
//             }
//
//             public void Dispose()
//             {
//             }
//
//             public void Reset()
//             {
//                 throw new NotImplementedException();
//             }
//
//             private static object GetModel(object container, ModelMetadata property)
//             {
//                 return property.PropertyGetter(container);
//             }
//
//             // Our property accessors don't work on Mono 4.0.4 - see https://github.com/aspnet/External/issues/44
//             // This is a workaround for what the PropertyGetter does in the background.
//             private static object GetModelOnMono(object container, string propertyName)
//             {
//                 var propertyInfo = container.GetType().GetRuntimeProperty(propertyName);
//                 try
//                 {
//                     return propertyInfo.GetValue(container);
//                 }
//                 catch (TargetInvocationException ex)
//                 {
//                     throw ex.InnerException;
//                 }
//             }
//         }
//     }
//
//     
//     /// <summary>
//     /// The default implementation of <see cref="IValidationStrategy"/> for a collection.
//     /// </summary>
//     /// <remarks>
//     /// This implementation handles cases like:
//     /// <example>
//     ///     Model: IList&lt;Student&gt;
//     ///     Query String: ?students[0].Age=8&amp;students[1].Age=9
//     ///
//     ///     In this case the elements of the collection are identified in the input data set by an incrementing
//     ///     integer index.
//     /// </example>
//     ///
//     /// or:
//     ///
//     /// <example>
//     ///     Model: IDictionary&lt;string, int&gt;
//     ///     Query String: ?students[0].Key=Joey&amp;students[0].Value=8
//     ///
//     ///     In this case the dictionary is treated as a collection of key-value pairs, and the elements of the
//     ///     collection are identified in the input data set by an incrementing integer index.
//     /// </example>
//     ///
//     /// Using this key format, the enumerator enumerates model objects of type matching
//     /// <see cref="ModelMetadata.ElementMetadata"/>. The indices of the elements in the collection are used to
//     /// compute the model prefix keys.
//     /// </remarks>
//     internal class JsonApiDefaultCollectionValidationStrategy : IValidationStrategy
//     {
//         private static readonly MethodInfo _getEnumerator = typeof(JsonApiDefaultCollectionValidationStrategy)
//             .GetMethod(nameof(GetEnumerator), BindingFlags.Static | BindingFlags.NonPublic);
//
//         /// <summary>
//         /// Gets an instance of <see cref="JsonApiDefaultCollectionValidationStrategy"/>.
//         /// </summary>
//         public static readonly JsonApiDefaultCollectionValidationStrategy Instance = new JsonApiDefaultCollectionValidationStrategy();
//         private readonly ConcurrentDictionary<Type, Func<object, IEnumerator>> _genericGetEnumeratorCache = new ConcurrentDictionary<Type, Func<object, IEnumerator>>();
//
//         private JsonApiDefaultCollectionValidationStrategy()
//         {
//         }
//
//         /// <inheritdoc />
//         public IEnumerator<ValidationEntry> GetChildren(
//             ModelMetadata metadata,
//             string key,
//             object model)
//         {
//             var enumerator = GetEnumeratorForElementType(metadata, model);
//             return new Enumerator(metadata.ElementMetadata, key, enumerator);
//         }
//
//         public IEnumerator GetEnumeratorForElementType(ModelMetadata metadata, object model)
//         {
//             Func<object, IEnumerator> getEnumerator = _genericGetEnumeratorCache.GetOrAdd(
//                 key: metadata.ElementType,
//                 valueFactory: (type) => {
//                     var getEnumeratorMethod = _getEnumerator.MakeGenericMethod(type);
//                     var parameter = Expression.Parameter(typeof(object), "model");
//                     var expression =
//                         Expression.Lambda<Func<object, IEnumerator>>(
//                             Expression.Call(null, getEnumeratorMethod, parameter),
//                             parameter);
//                     return expression.Compile();
//                 });
//
//             return getEnumerator(model);
//         }
//
//         // Called via reflection.
//         private static IEnumerator GetEnumerator<T>(object model)
//         {
//             return (model as IEnumerable<T>)?.GetEnumerator() ?? ((IEnumerable)model).GetEnumerator();
//         }
//
//         private class Enumerator : IEnumerator<ValidationEntry>
//         {
//             private readonly string _key;
//             private readonly ModelMetadata _metadata;
//             private readonly IEnumerator _enumerator;
//
//             private ValidationEntry _entry;
//             private int _index;
//
//             public Enumerator(
//                 ModelMetadata metadata,
//                 string key,
//                 IEnumerator enumerator)
//             {
//                 _metadata = metadata;
//                 _key = key;
//                 _enumerator = enumerator;
//
//                 _index = -1;
//             }
//
//             public ValidationEntry Current => _entry;
//
//             object IEnumerator.Current => Current;
//
//             public bool MoveNext()
//             {
//                 _index++;
//                 if (!_enumerator.MoveNext())
//                 {
//                     return false;
//                 }
//
//                 var key = ModelNames.CreateIndexModelName(_key, _index);
//                 var model = _enumerator.Current;
//
//                 _entry = new ValidationEntry(_metadata, key, model);
//
//                 return true;
//             }
//
//             public void Dispose()
//             {
//             }
//
//             public void Reset()
//             {
//                 _enumerator.Reset();
//             }
//         }
//     }
// }
