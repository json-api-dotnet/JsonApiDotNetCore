// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration
{
    internal sealed class JsonApiValidationVisitor : ValidationVisitor
    {
        // private readonly ValidationStack _currentPath;
        
        public JsonApiValidationVisitor(
            ActionContext actionContext,
            IModelValidatorProvider validatorProvider,
            ValidatorCache validatorCache,
            IModelMetadataProvider metadataProvider,
            ValidationStateDictionary validationState) 
            : base(actionContext, validatorProvider, validatorCache, metadataProvider, validationState) { }
        // {
        //     if (actionContext == null)
        //     {
        //         throw new ArgumentNullException(nameof(actionContext));
        //     }
        //
        //     if (validatorProvider == null)
        //     {
        //         throw new ArgumentNullException(nameof(validatorProvider));
        //     }
        //
        //     if (validatorCache == null)
        //     {
        //         throw new ArgumentNullException(nameof(validatorCache));
        //     }
        //     
        //     
        //     _currentPath = new ValidationStack();
        // }
        
        
        protected override bool VisitChildren(IValidationStrategy strategy)
        {
            var isValid = true;
            var enumerator = strategy.GetChildren(Metadata, Key, Model);
            var parentEntry = new ValidationEntry(Metadata, Key, Model);

            while (enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                var metadata = entry.Metadata;
                var key = entry.Key;
                
                var jsonApiFilter = metadata.PropertyValidationFilter as PartialPatchValidationFilter;
                var serviceProvider = Context?.HttpContext?.RequestServices;
                
                if (metadata.PropertyValidationFilter?.ShouldValidateEntry(entry, parentEntry) == false 
                    || jsonApiFilter != null && jsonApiFilter.ShouldValidateEntry(entry, parentEntry, serviceProvider) == false  )
                {
                    SuppressValidation(key);
                    continue;
                }

                isValid &= Visit(metadata, key, entry.Model);
            }

            return isValid;
        }
        

        // /// <summary>
        // /// Validates a object.
        // /// </summary>
        // /// <param name="metadata">The <see cref="ModelMetadata"/> associated with the model.</param>
        // /// <param name="key">The model prefix key.</param>
        // /// <param name="model">The model object.</param>
        // /// <param name="alwaysValidateAtTopLevel">If <c>true</c>, applies validation rules even if the top-level value is <c>null</c>.</param>
        // /// <returns><c>true</c> if the object is valid, otherwise <c>false</c>.</returns>
        // public override bool Validate(ModelMetadata metadata, string key, object model, bool alwaysValidateAtTopLevel)
        // {
        //     
        //     if (model == null && key != null && !alwaysValidateAtTopLevel)
        //     {
        //         var entry = ModelState[key];
        //
        //         // Rationale: We might see the same model state key for two different objects and want to preserve any
        //         // known invalidity.
        //         if (entry != null && entry.ValidationState != ModelValidationState.Invalid)
        //         {
        //             entry.ValidationState = ModelValidationState.Valid;
        //         }
        //
        //         return true;
        //     }
        //
        //     return Visit(metadata, key, model);
        // }

        // /// <summary>
        // /// Validates a single node in a model object graph.
        // /// </summary>
        // /// <returns><c>true</c> if the node is valid, otherwise <c>false</c>.</returns>
        // protected override bool ValidateNode()
        // {
        //     var state = ModelState.GetValidationState(Key);
        //
        //     // Rationale: we might see the same model state key used for two different objects.
        //     // We want to run validation unless it's already known that this key is invalid.
        //     if (state != ModelValidationState.Invalid)
        //     {
        //         var validators = Cache.GetValidators(Metadata, ValidatorProvider);
        //
        //         var count = validators.Count;
        //         if (count > 0)
        //         {
        //             var context = new ModelValidationContext(
        //                 Context,
        //                 Metadata,
        //                 MetadataProvider,
        //                 Container,
        //                 Model);
        //
        //             var results = new List<ModelValidationResult>();
        //             for (var i = 0; i < count; i++)
        //             {
        //                 results.AddRange(validators[i].Validate(context));
        //             }
        //
        //             var resultsCount = results.Count;
        //             for (var i = 0; i < resultsCount; i++)
        //             {
        //                 var result = results[i];
        //                 var key = ModelNames.CreatePropertyModelName(Key, result.MemberName);
        //
        //                 // It's OK for key to be the empty string here. This can happen when a top
        //                 // level object implements IValidatableObject.
        //                 ModelState.TryAddModelError(key, result.Message);
        //             }
        //         }
        //     }
        //
        //     state = ModelState.GetFieldValidationState(Key);
        //     if (state == ModelValidationState.Invalid)
        //     {
        //         return false;
        //     }
        //     else
        //     {
        //         // If the field has an entry in ModelState, then record it as valid. Don't create
        //         // extra entries if they don't exist already.
        //         var entry = ModelState[Key];
        //         if (entry != null)
        //         {
        //             entry.ValidationState = ModelValidationState.Valid;
        //         }
        //
        //         return true;
        //     }
        // }
        //
        // protected override bool Visit(ModelMetadata metadata, string key, object model)
        // {
        //     RuntimeHelpers.EnsureSufficientExecutionStack();
        //
        //     if (model != null && !_currentPath.Push(model))
        //     {
        //         // This is a cycle, bail.
        //         return true;
        //     }
        //
        //     if (MaxValidationDepth != null && _currentPath.Count > MaxValidationDepth)
        //     {
        //         // Non cyclic but too deep an object graph.
        //
        //         // Pop the current model to make ValidationStack.Dispose happy
        //         _currentPath.Pop(model);
        //
        //         string message;
        //         switch (metadata.MetadataKind)
        //         {
        //             case ModelMetadataKind.Property:
        //                 message = ResourcesInternal.FormatValidationVisitor_ExceededMaxPropertyDepth(nameof(JsonApiValidationVisitor), MaxValidationDepth, metadata.Name, metadata.ContainerType);
        //                 break;
        //
        //             default:
        //                 // Since the minimum depth is never 0, MetadataKind can never be Parameter. Consequently we only special case MetadataKind.Property.
        //                 message = ResourcesInternal.FormatValidationVisitor_ExceededMaxDepth(nameof(JsonApiValidationVisitor), MaxValidationDepth, metadata.ModelType);
        //                 break;
        //         }
        //
        //         message += " " + ResourcesInternal.FormatValidationVisitor_ExceededMaxDepthFix(nameof(MvcOptions), nameof(MvcOptions.MaxValidationDepth));
        //         throw new InvalidOperationException(message)
        //         {
        //             HelpLink = "https://aka.ms/AA21ue1",
        //         };
        //     }
        //
        //     var entry = GetValidationEntry(model);
        //     key = entry?.Key ?? key ?? string.Empty;
        //     metadata = entry?.Metadata ?? metadata;
        //     var strategy = entry?.Strategy;
        //
        //     if (ModelState.HasReachedMaxErrors)
        //     {
        //         SuppressValidation(key);
        //         return false;
        //     }
        //     else if (entry != null && entry.SuppressValidation)
        //     {
        //         // Use the key on the entry, because we might not have entries in model state.
        //         SuppressValidation(entry.Key);
        //         _currentPath.Pop(model);
        //         return true;
        //     }
        //     // If the metadata indicates that no validators exist AND the aggregate state for the key says that the model graph
        //     // is not invalid (i.e. is one of Unvalidated, Valid, or Skipped) we can safely mark the graph as valid.
        //     else if (metadata.HasValidators == false &&
        //         ModelState.GetFieldValidationState(key) != ModelValidationState.Invalid)
        //     {
        //         // No validators will be created for this graph of objects. Mark it as valid if it wasn't previously validated.
        //         var entries = ModelState.FindKeysWithPrefix(key);
        //         foreach (var item in entries)
        //         {
        //             if (item.Value.ValidationState == ModelValidationState.Unvalidated)
        //             {
        //                 item.Value.ValidationState = ModelValidationState.Valid;
        //             }
        //         }
        //
        //         _currentPath.Pop(model);
        //         return true;
        //     }
        //
        //     using (JsonApiStateManager.Recurse(this, key ?? string.Empty, metadata, model, strategy))
        //     {
        //         if (Metadata.IsEnumerableType)
        //         {
        //             return VisitComplexType(JsonApiDefaultCollectionValidationStrategy.Instance);
        //         }
        //
        //         if (Metadata.IsComplexType)
        //         {
        //             return VisitComplexType(JsonApiDefaultComplexObjectValidationStrategy.Instance);
        //         }
        //
        //         return VisitSimpleType();
        //     }
        // }
        //
        // // Covers everything VisitSimpleType does not i.e. both enumerations and complex types.
        // protected override bool VisitComplexType(IValidationStrategy defaultStrategy)
        // {
        //     var isValid = true;
        //     if (Model != null && Metadata.ValidateChildren)
        //     {
        //         var strategy = Strategy ?? defaultStrategy;
        //         isValid = VisitChildren(strategy);
        //     }
        //     else if (Model != null)
        //     {
        //         // Suppress validation for the entries matching this prefix. This will temporarily set
        //         // the current node to 'skipped' but we're going to visit it right away, so subsequent
        //         // code will set it to 'valid' or 'invalid'
        //         SuppressValidation(Key);
        //     }
        //
        //     // Double-checking HasReachedMaxErrors just in case this model has no properties.
        //     // If validation has failed for any children, only validate the parent if ValidateComplexTypesIfChildValidationFails is true.
        //     if ((isValid || ValidateComplexTypesIfChildValidationFails) && !ModelState.HasReachedMaxErrors)
        //     {
        //         isValid &= ValidateNode();
        //     }
        //
        //     return isValid;
        // }
        //
        // protected override bool VisitSimpleType()
        // {
        //     if (ModelState.HasReachedMaxErrors)
        //     {
        //         SuppressValidation(Key);
        //         return false;
        //     }
        //
        //     return ValidateNode();
        // }
        //
        // protected override void SuppressValidation(string key)
        // {
        //     if (key == null)
        //     {
        //         // If the key is null, that means that we shouldn't expect any entries in ModelState for
        //         // this value, so there's nothing to do.
        //         return;
        //     }
        //
        //     var entries = ModelState.FindKeysWithPrefix(key);
        //     foreach (var entry in entries)
        //     {
        //         if (entry.Value.ValidationState != ModelValidationState.Invalid)
        //         {
        //             entry.Value.ValidationState = ModelValidationState.Skipped;
        //         }
        //     }
        // }
        //
        // protected override ValidationStateEntry GetValidationEntry(object model)
        // {
        //     if (model == null || ValidationState == null)
        //     {
        //         return null;
        //     }
        //
        //     ValidationState.TryGetValue(model, out var entry);
        //     return entry;
        // }
        //
        // protected readonly struct JsonApiStateManager : IDisposable
        // {
        //     private readonly JsonApiValidationVisitor _visitor;
        //     private readonly object _container;
        //     private readonly string _key;
        //     private readonly ModelMetadata _metadata;
        //     private readonly object _model;
        //     private readonly object _newModel;
        //     private readonly IValidationStrategy _strategy;
        //
        //     public static JsonApiStateManager Recurse(
        //         JsonApiValidationVisitor visitor,
        //         string key,
        //         ModelMetadata metadata,
        //         object model,
        //         IValidationStrategy strategy)
        //     {
        //         var recursifier = new JsonApiStateManager(visitor, model);
        //
        //         visitor.Container = visitor.Model;
        //         visitor.Key = key;
        //         visitor.Metadata = metadata;
        //         visitor.Model = model;
        //         visitor.Strategy = strategy;
        //
        //         return recursifier;
        //     }
        //
        //     public JsonApiStateManager(JsonApiValidationVisitor visitor, object newModel)
        //     {
        //         _visitor = visitor;
        //         _newModel = newModel;
        //
        //         _container = _visitor.Container;
        //         _key = _visitor.Key;
        //         _metadata = _visitor.Metadata;
        //         _model = _visitor.Model;
        //         _strategy = _visitor.Strategy;
        //     }
        //
        //     public void Dispose()
        //     {
        //         _visitor.Container = _container;
        //         _visitor.Key = _key;
        //         _visitor.Metadata = _metadata;
        //         _visitor.Model = _model;
        //         _visitor.Strategy = _strategy;
        //
        //         _visitor._currentPath.Pop(_newModel);
        //     }
        // }
    }
}
