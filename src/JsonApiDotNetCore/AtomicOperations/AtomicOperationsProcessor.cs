using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public class AtomicOperationsProcessor : IAtomicOperationsProcessor
    {
        private readonly IAtomicOperationProcessorResolver _resolver;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly DbContext _dbContext;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;

        public AtomicOperationsProcessor(IAtomicOperationProcessorResolver resolver, ILocalIdTracker localIdTracker,
            IJsonApiRequest request, ITargetedFields targetedFields, IResourceContextProvider resourceContextProvider,
            IEnumerable<IDbContextResolver> dbContextResolvers)
        {
            if (dbContextResolvers == null) throw new ArgumentNullException(nameof(dbContextResolvers));

            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));

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

            var results = new List<AtomicResultObject>();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var operation in operations)
                {
                    // TODO: @OPS: Do we need to keep this in?
                    // _dbContext.ResetChangeTracker();

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
                    error.Source.Pointer = $"/atomic:operations[{results.Count}]";
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

        private async Task<AtomicResultObject> ProcessOperation(AtomicOperationObject operation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string resourceName = null;
            if (operation.Code == AtomicOperationCode.Add || operation.Code == AtomicOperationCode.Update)
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
                else if (operation.ManyData != null)
                {
                    foreach (var resourceObject in operation.ManyData)
                    {
                        resourceName = resourceObject.Type;
                        if (resourceName == null)
                        {
                            throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                            {
                                Title = "The data.type element is required."
                            });
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("TODO: Data is missing.");
                }
            }

            if (operation.Code == AtomicOperationCode.Remove)
            {
                resourceName = operation.Ref?.Type;
                if (resourceName == null)
                {
                    throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = "The ref.type element is required."
                    });
                }
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

            _targetedFields.Attributes.Clear();
            _targetedFields.Relationships.Clear();

            if (operation.Ref?.Relationship != null)
            {
                var primaryResourceContext = _resourceContextProvider.GetResourceContext(operation.Ref.Type);
                var requestRelationship = primaryResourceContext.Relationships.SingleOrDefault(relationship => relationship.PublicName == operation.Ref.Relationship);

                if (requestRelationship == null)
                {
                    throw new InvalidOperationException("TODO: Relationship does not exist.");
                }

                ((JsonApiRequest)_request).PrimaryResource = primaryResourceContext;
                ((JsonApiRequest)_request).PrimaryId = operation.Ref.Id;
                ((JsonApiRequest)_request).Relationship = requestRelationship;
                ((JsonApiRequest)_request).SecondaryResource = _resourceContextProvider.GetResourceContext(requestRelationship.RightType);

                _targetedFields.Relationships.Add(_request.Relationship);
            }
            else
            {
                ((JsonApiRequest)_request).PrimaryResource = resourceContext;
                ((JsonApiRequest)_request).PrimaryId = null;
                ((JsonApiRequest)_request).Relationship = null;
                ((JsonApiRequest)_request).SecondaryResource = null;
            }

            var processor = _resolver.ResolveProcessor(operation);
            return await processor.ProcessAsync(operation, cancellationToken);
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
