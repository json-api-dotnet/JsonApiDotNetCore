using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

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
        private readonly ICurrentRequest _currentRequest;
        private readonly IContextEntityProvider _provider;

        public SparseFieldsService(ICurrentRequest currentRequest, IContextEntityProvider provider)
        {
            _selectedFields = new List<AttrAttribute>();
            _selectedRelationshipFields = new Dictionary<RelationshipAttribute, List<AttrAttribute>>();
            _currentRequest = currentRequest;
            _provider = provider;
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
        public override void Parse(string key, string value)
        {
            var primaryResource = _currentRequest.GetRequestResource();

            // expected: fields[TYPE]=prop1,prop2
            var typeName = key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];
            var includedFields = new List<string> { nameof(Identifiable.Id) };

            var relationship = primaryResource.Relationships.SingleOrDefault(a => a.Is(typeName));
            if (relationship == null && string.Equals(typeName, primaryResource.EntityName, StringComparison.OrdinalIgnoreCase) == false)
                throw new JsonApiException(400, $"fields[{typeName}] is invalid");

            var fields = value.Split(QueryConstants.COMMA);
            foreach (var field in fields)
            {
                if (relationship != default)
                {
                    var relationProperty = _provider.GetContextEntity(relationship.DependentType);
                    var attr = relationProperty.Attributes.SingleOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(400, $"'{relationship.DependentType.Name}' does not contain '{field}'.");

                    if (!_selectedRelationshipFields.TryGetValue(relationship, out var registeredFields))
                        _selectedRelationshipFields.Add(relationship, registeredFields = new List<AttrAttribute>());
                    registeredFields.Add(attr);
                }
                else
                {
                    var attr = primaryResource.Attributes.SingleOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(400, $"'{primaryResource.EntityName}' does not contain '{field}'.");

                    (_selectedFields = _selectedFields ?? new List<AttrAttribute>()).Add(attr);
                }
            }
        }
    }
}
