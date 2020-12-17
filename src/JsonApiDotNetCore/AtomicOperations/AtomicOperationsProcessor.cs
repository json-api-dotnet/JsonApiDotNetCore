using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public class AtomicOperationsProcessor : IAtomicOperationsProcessor
    {
        private readonly IAtomicOperationProcessorResolver _resolver;
        private readonly IJsonApiOptions _options;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly DbContext _dbContext;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IObjectModelValidator _validator;
        private readonly IJsonApiDeserializer _deserializer;

        public AtomicOperationsProcessor(IAtomicOperationProcessorResolver resolver, IJsonApiOptions options,
            ILocalIdTracker localIdTracker, IJsonApiRequest request, ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider, IEnumerable<IDbContextResolver> dbContextResolvers,
            IObjectModelValidator validator, IJsonApiDeserializer deserializer)
        {
            if (dbContextResolvers == null) throw new ArgumentNullException(nameof(dbContextResolvers));

            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));

            var resolvers = dbContextResolvers.ToArray();
            if (resolvers.Length != 1)
            {
                throw new InvalidOperationException(
                    "TODO: At least one DbContext is required for atomic operations. Multiple DbContexts are currently not supported.");
            }

            _dbContext = resolvers[0].GetContext();
        }

        public async Task<IList<AtomicResultObject>> ProcessAsync(IList<AtomicOperationObject> operations, CancellationToken cancellationToken)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            if (_options.ValidateModelState)
            {
                ValidateModelState(operations);
            }

            var results = new List<AtomicResultObject>();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var operation in operations)
                {
                    _dbContext.ResetChangeTracker();

                    var result = await ProcessOperation(operation, cancellationToken);
                    results.Add(result);
                }

                await transaction.CommitAsync(cancellationToken);
                return results;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonApiException exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                foreach (var error in exception.Errors)
                {
                    error.Source.Pointer = $"/atomic:operations[{results.Count}]" + error.Source.Pointer;
                }

                throw;
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
                {
                    Title = "An unhandled error occurred while processing an atomic operation in this request.",
                    Detail = exception.Message,
                    Source =
                    {
                        Pointer = $"/atomic:operations[{results.Count}]"
                    }

                }, exception);
            }
        }

        private void ValidateModelState(IEnumerable<AtomicOperationObject> operations)
        {
            var violations = new List<ModelStateViolation>();

            int index = 0;
            foreach (var operation in operations)
            {
                if (operation.Ref?.Relationship == null && operation.SingleData != null)
                {
                    PrepareForOperation(operation);

                    var validationContext = new ActionContext();

                    var model = _deserializer.CreateResourceFromObject(operation.SingleData);
                    _validator.Validate(validationContext, null, string.Empty, model);

                    if (!validationContext.ModelState.IsValid)
                    {
                        foreach (var (key, entry) in validationContext.ModelState)
                        {
                            foreach (var error in entry.Errors)
                            {
                                var violation = new ModelStateViolation($"/atomic:operations[{index}]/data/attributes/", key, model.GetType(), error);
                                violations.Add(violation);
                            }
                        }
                    }
                }

                index++;
            }

            if (violations.Any())
            {
                var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                throw new InvalidModelStateException(violations, _options.IncludeExceptionStackTraceInErrors, namingStrategy);
            }
        }

        private async Task<AtomicResultObject> ProcessOperation(AtomicOperationObject operation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string resourceName = null;

            if (operation.Ref != null)
            {
                resourceName = operation.Ref.Type;
            }
            else
            {
                if (operation.SingleData != null)
                {
                    resourceName = operation.SingleData.Type;
                    if (resourceName == null)
                    {
                        throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                        {
                            Title = "The data.type element is required."
                        });
                    }
                }
                else if (operation.ManyData != null && operation.ManyData.Any())
                {
                    foreach (var resourceObject in operation.ManyData)
                    {
                        resourceName = resourceObject.Type;
                        if (resourceName == null)
                        {
                            throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                            {
                                Title = "The data[].type element is required."
                            });
                        }
                        
                        // TODO: Verify all are of the same (or compatible) type.
                    }
                }
            }

            if (resourceName == null)
            {
                throw new InvalidOperationException("TODO: Failed to determine targeted resource.");
            }
            
            bool isResourceAdd = operation.Code == AtomicOperationCode.Add && operation.Ref == null;

            if (isResourceAdd && operation.SingleData?.Lid != null)
            {
                _localIdTracker.Declare(operation.SingleData.Lid, operation.SingleData.Type);
            }

            ReplaceLocalIdsInOperationObject(operation, isResourceAdd);

            var resourceContext = _resourceContextProvider.GetResourceContext(resourceName);
            if (resourceContext == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Request body includes unknown resource type.",
                    Detail = $"Resource type '{resourceName}' does not exist."
                });
            }

            PrepareForOperation(operation);

            var processor = _resolver.ResolveProcessor(operation);
            return await processor.ProcessAsync(operation, cancellationToken);
        }

        private void PrepareForOperation(AtomicOperationObject operation)
        {
            _targetedFields.Attributes.Clear();
            _targetedFields.Relationships.Clear();

            var resourceName = operation.GetResourceTypeName();
            var primaryResourceContext = _resourceContextProvider.GetResourceContext(resourceName);

            ((JsonApiRequest) _request).OperationCode = operation.Code;
            ((JsonApiRequest)_request).PrimaryResource = primaryResourceContext;

            if (operation.Ref != null)
            {
                ((JsonApiRequest)_request).PrimaryId = operation.Ref.Id;

                if (operation.Ref?.Relationship != null)
                {
                    var relationship = primaryResourceContext.Relationships.SingleOrDefault(relationship => relationship.PublicName == operation.Ref.Relationship);
                    if (relationship == null)
                    {
                        throw new InvalidOperationException("TODO: Relationship does not exist.");
                    }

                    var secondaryResource = _resourceContextProvider.GetResourceContext(relationship.RightType);
                    if (secondaryResource == null)
                    {
                        throw new InvalidOperationException("TODO: Secondary resource does not exist.");
                    }

                    ((JsonApiRequest)_request).Relationship = relationship;
                    ((JsonApiRequest)_request).SecondaryResource = secondaryResource;

                    _targetedFields.Relationships.Add(relationship);
                }
            }
            else
            {
                ((JsonApiRequest)_request).PrimaryId = null;
                ((JsonApiRequest)_request).Relationship = null;
                ((JsonApiRequest)_request).SecondaryResource = null;
            }
        }

        private void ReplaceLocalIdsInOperationObject(AtomicOperationObject operation, bool isResourceAdd)
        {
            if (operation.Ref != null)
            {
                ReplaceLocalIdInResourceIdentifierObject(operation.Ref);
            }

            if (operation.SingleData != null)
            {
                ReplaceLocalIdsInResourceObject(operation.SingleData, isResourceAdd);
            }

            if (operation.ManyData != null)
            {
                foreach (var resourceObject in operation.ManyData)
                {
                    ReplaceLocalIdsInResourceObject(resourceObject, isResourceAdd);
                }
            }
        }

        private void ReplaceLocalIdsInResourceObject(ResourceObject resourceObject, bool isResourceAdd)
        {
            if (!isResourceAdd)
            {
                ReplaceLocalIdInResourceIdentifierObject(resourceObject);
            }

            if (resourceObject.Relationships != null)
            {
                foreach (var relationshipEntry in resourceObject.Relationships.Values)
                {
                    if (relationshipEntry.IsManyData)
                    {
                        foreach (var relationship in relationshipEntry.ManyData)
                        {
                            ReplaceLocalIdInResourceIdentifierObject(relationship);
                        }
                    }
                    else
                    {
                        var relationship = relationshipEntry.SingleData;

                        if (relationship != null)
                        {
                            ReplaceLocalIdInResourceIdentifierObject(relationship);
                        }
                    }
                }
            }
        }

        private void ReplaceLocalIdInResourceIdentifierObject(ResourceIdentifierObject resourceIdentifierObject)
        {
            if (resourceIdentifierObject.Lid != null)
            {
                resourceIdentifierObject.Id =
                    _localIdTracker.GetValue(resourceIdentifierObject.Lid, resourceIdentifierObject.Type);
            }
        }
    }
}
