using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class SparseFieldsService : QueryParameterService, ISparseFieldsService
    {
        /// <summary>
        /// The selected fields for the primary resource of this request.
        /// </summary>
        private List<AttrAttribute> _selectedFields;
        /// <summary>
        /// The selected field for any included relationships
        /// </summary>
        private readonly Dictionary<RelationshipAttribute, List<AttrAttribute>> _selectedRelationshipFields;

        public override string Name => "fields";

        public SparseFieldsService(IResourceGraph resourceGraph, ICurrentRequest currentRequest) : base(resourceGraph, currentRequest)
        {
            _selectedFields = new List<AttrAttribute>();
            _selectedRelationshipFields = new Dictionary<RelationshipAttribute, List<AttrAttribute>>();
        }

        /// <inheritdoc/>
        public List<AttrAttribute> Get(RelationshipAttribute relationship = null)
        {
            if (relationship == null)
                return _selectedFields;

            _selectedRelationshipFields.TryGetValue(relationship, out var fields);
            return fields;
        }

        /// <inheritdoc/>
        public virtual void Parse(KeyValuePair<string, StringValues> queryParameter)
        {
            // expected: fields[TYPE]=prop1,prop2
            var typeName = queryParameter.Key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];
            var fields = new List<string> { nameof(Identifiable.Id) };

            var relationship = _requestResource.Relationships.FirstOrDefault(a => a.Is(typeName));
            if (relationship == null && string.Equals(typeName, _requestResource.EntityName, StringComparison.OrdinalIgnoreCase) == false)
                throw new JsonApiException(400, $"fields[{typeName}] is invalid");

            fields.AddRange(((string)queryParameter.Value).Split(QueryConstants.COMMA));
            foreach (var field in fields)
            {
                if (relationship != default)
                {
                    var relationProperty = _resourceGraph.GetContextEntity(relationship.DependentType);
                    var attr = relationProperty.Attributes.FirstOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(400, $"'{relationship.DependentType.Name}' does not contain '{field}'.");

                    if (!_selectedRelationshipFields.TryGetValue(relationship, out var registeredFields))
                        _selectedRelationshipFields.Add(relationship, registeredFields = new List<AttrAttribute>());
                    registeredFields.Add(attr);
                }
                else
                {
                    var attr = _requestResource.Attributes.FirstOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(400, $"'{_requestResource.EntityName}' does not contain '{field}'.");

                    (_selectedFields = _selectedFields ?? new List<AttrAttribute>()).Add(attr);
                }
            }
        }
    }
}
