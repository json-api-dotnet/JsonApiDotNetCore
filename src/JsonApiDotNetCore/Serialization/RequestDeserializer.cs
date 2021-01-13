using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Server deserializer implementation of the <see cref="BaseDeserializer"/>.
    /// </summary>
    public class RequestDeserializer : BaseDeserializer, IJsonApiDeserializer
    {
        private readonly ITargetedFields  _targetedFields;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJsonApiRequest _request;

        public RequestDeserializer(
            IResourceContextProvider resourceContextProvider,
            IResourceFactory resourceFactory,
            ITargetedFields targetedFields,
            IHttpContextAccessor httpContextAccessor,
            IJsonApiRequest request) 
            : base(resourceContextProvider, resourceFactory)
        {
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <inheritdoc />
        public object DeserializeDocument(string body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));

            if (_request.Kind == EndpointKind.Relationship)
            {
                _targetedFields.Relationships.Add(_request.Relationship);
            }

            if (_request.Kind == EndpointKind.AtomicOperations)
            {
                return DeserializeOperationsDocument(body);
            }

            var instance = DeserializeBody(body);

            AssertResourceIdIsNotTargeted(_targetedFields);

            return instance;
        }

        private object DeserializeOperationsDocument(string body)
        {
            JToken bodyToken = LoadJToken(body);
            var document = bodyToken.ToObject<AtomicOperationsDocument>();

            var operations = new List<OperationContainer>();

            if (document?.Operations == null || !document.Operations.Any())
            {
                throw new JsonApiSerializationException("No operations found.", null);
            }

            AtomicOperationIndex = 0;
            foreach (var operation in document.Operations)
            {
                var container = DeserializeOperation(operation);
                operations.Add(container);

                AtomicOperationIndex++;
            }

            return operations;
        }

        // TODO: Cleanup code.

        private OperationContainer DeserializeOperation(AtomicOperationObject operation)
        {
            if (operation.Href != null)
            {
                throw new JsonApiSerializationException("Usage of the 'href' element is not supported.", null,
                    atomicOperationIndex: AtomicOperationIndex);
            }

            var kind = GetOperationKind(operation);

            if (kind == OperationKind.AddToRelationship && operation.Ref.Relationship == null)
            {
                throw new JsonApiSerializationException("The 'ref.relationship' element is required.", null,
                    atomicOperationIndex: AtomicOperationIndex);
            }

            RelationshipAttribute relationshipInRef = null;
            ResourceContext resourceContextInRef = null;

            if (operation.Ref != null)
            {
                if (operation.Ref.Type == null)
                {
                    throw new JsonApiSerializationException("The 'ref.type' element is required.", null,
                        atomicOperationIndex: AtomicOperationIndex);
                }

                resourceContextInRef = GetExistingResourceContext(operation.Ref.Type);
                
                if ((operation.Ref.Id == null && operation.Ref.Lid == null) || (operation.Ref.Id != null && operation.Ref.Lid != null))
                {
                    throw new JsonApiSerializationException("The 'ref.id' or 'ref.lid' element is required.", null,
                        atomicOperationIndex: AtomicOperationIndex);
                }

                if (operation.Ref.Id != null)
                {
                    try
                    {
                        TypeHelper.ConvertType(operation.Ref.Id, resourceContextInRef.IdentityType);
                    }
                    catch (FormatException exception)
                    {
                        throw new JsonApiSerializationException(null, exception.Message, null, AtomicOperationIndex);
                    }
                }

                if (operation.Ref.Relationship != null)
                {
                    relationshipInRef = resourceContextInRef.Relationships.FirstOrDefault(r => r.PublicName == operation.Ref.Relationship);
                    if (relationshipInRef == null)
                    {
                        throw new JsonApiSerializationException(
                            "The referenced relationship does not exist.",
                            $"Resource of type '{operation.Ref.Type}' does not contain a relationship named '{operation.Ref.Relationship}'.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    if ((kind == OperationKind.AddToRelationship || kind == OperationKind.RemoveFromRelationship) && relationshipInRef is HasOneAttribute)
                    {
                        throw new JsonApiSerializationException(
                            $"Only to-many relationships can be targeted in '{operation.Code.ToString().Camelize()}' operations.",
                            $"Relationship '{operation.Ref.Relationship}' must be a to-many relationship.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    if (relationshipInRef is HasOneAttribute && operation.ManyData != null)
                    {
                        throw new JsonApiSerializationException(
                            "Expected single data element for to-one relationship.",
                            $"Expected single data element for '{relationshipInRef.PublicName}' relationship.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    if (relationshipInRef is HasManyAttribute && operation.ManyData == null)
                    {
                        throw new JsonApiSerializationException(
                            "Expected data[] element for to-many relationship.",
                            $"Expected data[] element for '{relationshipInRef.PublicName}' relationship.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }
                }
            }

            if (operation.ManyData != null)
            {
                foreach (var resourceObject in operation.ManyData)
                {
                    if (relationshipInRef == null)
                    {
                        throw new JsonApiSerializationException("Expected single data element for create/update resource operation.",
                            null, atomicOperationIndex: AtomicOperationIndex);
                    }

                    if (resourceObject.Type == null)
                    {
                        throw new JsonApiSerializationException("The 'data[].type' element is required.", null,
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    if ((resourceObject.Id == null && resourceObject.Lid == null) || (resourceObject.Id != null && resourceObject.Lid != null))
                    {
                        throw new JsonApiSerializationException("The 'data[].id' or 'data[].lid' element is required.", null,
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    var rightResourceContext = GetExistingResourceContext(resourceObject.Type);
                    if (!rightResourceContext.ResourceType.IsAssignableFrom(relationshipInRef.RightType))
                    {
                        var relationshipRightTypeName = ResourceContextProvider.GetResourceContext(relationshipInRef.RightType);
                            
                        throw new JsonApiSerializationException("Resource type mismatch between 'ref.relationship' and 'data[].type' element.", 
                            $@"Expected resource of type '{relationshipRightTypeName}' in 'data[].type', instead of '{rightResourceContext.PublicName}'.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }
                }
            }

            if (operation.SingleData != null)
            {
                var resourceObject = operation.SingleData;

                if (resourceObject.Type == null)
                {
                    throw new JsonApiSerializationException("The 'data.type' element is required.", null,
                        atomicOperationIndex: AtomicOperationIndex);
                }

                if (kind != OperationKind.CreateResource)
                {
                    if ((resourceObject.Id == null && resourceObject.Lid == null) || (resourceObject.Id != null && resourceObject.Lid != null))
                    {
                        throw new JsonApiSerializationException("The 'data.id' or 'data.lid' element is required.", null,
                            atomicOperationIndex: AtomicOperationIndex);
                    }
                }

                var resourceContextInData = GetExistingResourceContext(resourceObject.Type);

                if (kind.IsRelationship() && relationshipInRef != null)
                {
                    var rightResourceContext = resourceContextInData;
                    if (!rightResourceContext.ResourceType.IsAssignableFrom(relationshipInRef.RightType))
                    {
                        var relationshipRightTypeName = ResourceContextProvider.GetResourceContext(relationshipInRef.RightType);
                            
                        throw new JsonApiSerializationException("Resource type mismatch between 'ref.relationship' and 'data.type' element.", 
                            $@"Expected resource of type '{relationshipRightTypeName}' in 'data.type', instead of '{rightResourceContext.PublicName}'.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }
                }
                else 
                {
                    if (resourceContextInRef != null && resourceContextInRef != resourceContextInData)
                    {
                        throw new JsonApiSerializationException("Resource type mismatch between 'ref.type' and 'data.type' element.", 
                            $@"Expected resource of type '{resourceContextInRef.PublicName}' in 'data.type', instead of '{resourceContextInData.PublicName}'.",
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    if (operation.Ref != null)
                    {
                        if (operation.Ref.Id != null && resourceObject.Id != null && resourceObject.Id != operation.Ref.Id)
                        {
                            throw new JsonApiSerializationException("Resource ID mismatch between 'ref.id' and 'data.id' element.", 
                                $@"Expected resource with ID '{operation.Ref.Id}' in 'data.id', instead of '{resourceObject.Id}'.",
                                atomicOperationIndex: AtomicOperationIndex);
                        }

                        if (operation.Ref.Lid != null && resourceObject.Lid != null && resourceObject.Lid != operation.Ref.Lid)
                        {
                            throw new JsonApiSerializationException("Resource local ID mismatch between 'ref.lid' and 'data.lid' element.", 
                                $@"Expected resource with local ID '{operation.Ref.Lid}' in 'data.lid', instead of '{resourceObject.Lid}'.",
                                atomicOperationIndex: AtomicOperationIndex);
                        }

                        if (operation.Ref.Id != null && resourceObject.Lid != null)
                        {
                            throw new JsonApiSerializationException("Resource identity mismatch between 'ref.id' and 'data.lid' element.", 
                                $@"Expected resource with ID '{operation.Ref.Id}' in 'data.id', instead of '{resourceObject.Lid}' in 'data.lid'.",
                                atomicOperationIndex: AtomicOperationIndex);
                        }

                        if (operation.Ref.Lid != null && resourceObject.Id != null)
                        {
                            throw new JsonApiSerializationException("Resource identity mismatch between 'ref.lid' and 'data.id' element.", 
                                $@"Expected resource with local ID '{operation.Ref.Lid}' in 'data.lid', instead of '{resourceObject.Id}' in 'data.id'.",
                                atomicOperationIndex: AtomicOperationIndex);
                        }
                    }
                }
            }

            return ToOperationContainer(operation, kind);
        }

        private OperationContainer ToOperationContainer(AtomicOperationObject operation, OperationKind kind)
        {
            var resourceName = operation.GetResourceTypeName();
            var primaryResourceContext = GetExistingResourceContext(resourceName);

            _targetedFields.Attributes.Clear();
            _targetedFields.Relationships.Clear();

            IIdentifiable resource;

            switch (kind)
            {
                case OperationKind.CreateResource:
                case OperationKind.UpdateResource:
                {
                    // TODO: @OPS: Chicken-and-egg problem: ParseResourceObject depends on _request.OperationKind, which is not built yet.
                    ((JsonApiRequest) _request).OperationKind = kind;

                    resource = ParseResourceObject(operation.SingleData);
                    break;
                }
                case OperationKind.DeleteResource:
                case OperationKind.SetRelationship:
                case OperationKind.AddToRelationship:
                case OperationKind.RemoveFromRelationship:
                {
                    resource = ResourceFactory.CreateInstance(primaryResourceContext.ResourceType);
                    resource.StringId = operation.Ref.Id;
                    resource.LocalId = operation.Ref.Lid;
                    break;
                }
                default:
                {
                    throw new NotSupportedException($"Unknown operation kind '{kind}'.");
                }
            }

            var request = new JsonApiRequest
            {
                Kind = EndpointKind.AtomicOperations,
                OperationKind = kind,
                PrimaryId = resource.StringId,
                BasePath = "TODO: Set this...",
                PrimaryResource = primaryResourceContext
            };

            if (operation.Ref?.Relationship != null)
            {
                var relationship = primaryResourceContext.Relationships.Single(r => r.PublicName == operation.Ref.Relationship);

                var secondaryResourceContext = ResourceContextProvider.GetResourceContext(relationship.RightType);
                if (secondaryResourceContext == null)
                {
                    throw new InvalidOperationException("TODO: @OPS: Secondary resource type does not exist.");
                }

                request.SecondaryResource = secondaryResourceContext;
                request.Relationship = relationship;
                request.IsCollection = relationship is HasManyAttribute;

                _targetedFields.Relationships.Add(relationship);

                if (operation.SingleData != null)
                {
                    var rightResource = ParseResourceObject(operation.SingleData);
                    relationship.SetValue(resource, rightResource);
                }
                else if (operation.ManyData != null)
                {
                    var secondaryResources = operation.ManyData.Select(ParseResourceObject).ToArray();
                    var rightResources = TypeHelper.CopyToTypedCollection(secondaryResources, relationship.Property.PropertyType);
                    relationship.SetValue(resource, rightResources);
                }
            }

            var targetedFields = new TargetedFields
            {
                Attributes = _targetedFields.Attributes.ToHashSet(),
                Relationships = _targetedFields.Relationships.ToHashSet()
            };

            AssertResourceIdIsNotTargeted(targetedFields);

            return new OperationContainer(kind, resource, targetedFields, request);
        }

        private OperationKind GetOperationKind(AtomicOperationObject operation)
        {
            switch (operation.Code)
            {
                case AtomicOperationCode.Add:
                {
                    return operation.Ref != null ? OperationKind.AddToRelationship : OperationKind.CreateResource;
                }
                case AtomicOperationCode.Update:
                {
                    return operation.Ref?.Relationship != null ? OperationKind.SetRelationship : OperationKind.UpdateResource;
                }
                case AtomicOperationCode.Remove:
                {
                    if (operation.Ref == null)
                    {
                        throw new JsonApiSerializationException("The 'ref' element is required.", null,
                            atomicOperationIndex: AtomicOperationIndex);
                    }

                    return operation.Ref.Relationship != null ? OperationKind.RemoveFromRelationship : OperationKind.DeleteResource;
                }
                default:
                {
                    throw new NotSupportedException($"Unknown operation code '{operation.Code}'.");
                }
            }
        }

        private void AssertResourceIdIsNotTargeted(ITargetedFields targetedFields)
        {
            if (!_request.IsReadOnly && targetedFields.Attributes.Any(attribute => attribute.Property.Name == nameof(Identifiable.Id)))
            {
                throw new JsonApiSerializationException("Resource ID is read-only.", null,
                    atomicOperationIndex: AtomicOperationIndex);
            }
        }

        /// <summary>
        /// Additional processing required for server deserialization. Flags a
        /// processed attribute or relationship as updated using <see cref="ITargetedFields"/>.
        /// </summary>
        /// <param name="resource">The resource that was constructed from the document's body.</param>
        /// <param name="field">The metadata for the exposed field.</param>
        /// <param name="data">Relationship data for <paramref name="resource"/>. Is null when <paramref name="field"/> is not a <see cref="RelationshipAttribute"/>.</param>
        protected override void AfterProcessField(IIdentifiable resource, ResourceFieldAttribute field, RelationshipEntry data = null)
        {
            bool isCreatingResource = IsCreatingResource();
            bool isUpdatingResource = IsUpdatingResource();

            if (field is AttrAttribute attr)
            {
                if (isCreatingResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowCreate))
                {
                    throw new JsonApiSerializationException(
                        "Setting the initial value of the requested attribute is not allowed.",
                        $"Setting the initial value of '{attr.PublicName}' is not allowed.",
                        atomicOperationIndex: AtomicOperationIndex);
                }

                if (isUpdatingResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowChange))
                {
                    throw new JsonApiSerializationException(
                        "Changing the value of the requested attribute is not allowed.",
                        $"Changing the value of '{attr.PublicName}' is not allowed.",
                        atomicOperationIndex: AtomicOperationIndex);
                }

                _targetedFields.Attributes.Add(attr);
            }
            else if (field is RelationshipAttribute relationship)
                _targetedFields.Relationships.Add(relationship);
        }

        private bool IsCreatingResource()
        {
            return _request.Kind == EndpointKind.AtomicOperations
                ? _request.OperationKind == OperationKind.CreateResource
                : _request.Kind == EndpointKind.Primary &&
                  _httpContextAccessor.HttpContext.Request.Method == HttpMethod.Post.Method;
        }

        private bool IsUpdatingResource()
        {
            return _request.Kind == EndpointKind.AtomicOperations
                ? _request.OperationKind == OperationKind.UpdateResource
                : _request.Kind == EndpointKind.Primary &&
                  _httpContextAccessor.HttpContext.Request.Method == HttpMethod.Patch.Method;
        }
    }
}
