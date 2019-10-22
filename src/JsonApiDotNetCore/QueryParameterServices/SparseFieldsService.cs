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

        public SparseFieldsService(IContextEntityProvider contextEntityProvider, ICurrentRequest currentRequest) : base(contextEntityProvider, currentRequest)
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
        {   // expected: articles?fields=prop1,prop2
            //           articles?fields[articles]=prop1,prop2  <-- this form in invalid UNLESS "articles" is actually a relationship on Article
            //           articles?fields[relationship]=prop1,prop2
            var fields = new List<string> { nameof(Identifiable.Id) };
            fields.AddRange(((string)queryParameter.Value).Split(QueryConstants.COMMA));

            var keySplitted = queryParameter.Key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET);

            if (keySplitted.Count() == 1)
            {   // input format: fields=prop1,prop2
                foreach (var field in fields)
                    RegisterRequestResourceField(field);
            }
            else
            {  // input format: fields[articles]=prop1,prop2
                string navigation = keySplitted[1];
                // it is possible that the request resource has a relationship
                // that is equal to the resource name, like with self-referering data types (eg directory structures)
                // if not, no longer support this type of sparse field selection.
                if (navigation == _requestResource.EntityName && !_requestResource.Relationships.Any(a => a.Is(navigation)))
                    throw new JsonApiException(400, $"Use \"?fields=...\" instead of \"fields[{navigation}]\":" +
                        $" the square bracket navigations is now reserved " +
                        $"for relationships only. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/555#issuecomment-543100865");

                if (navigation.Contains(QueryConstants.DOT))
                    throw new JsonApiException(400, $"fields[{navigation}] is not valid: deeply nested sparse field selection is not yet supported.");

                var relationship = _requestResource.Relationships.SingleOrDefault(a => a.Is(navigation));
                if (relationship == null)
                    throw new JsonApiException(400, $"\"{navigation}\" in \"fields[{navigation}]\" is not a valid relationship of {_requestResource.EntityName}");

                foreach (var field in fields)
                    RegisterRelatedResourceField(field, relationship);

            }
        }

        /// <summary>
        /// Registers field selection queries of the form articles?fields[author]=first-name
        /// </summary>
        private void RegisterRelatedResourceField(string field, RelationshipAttribute relationship)
        {
            var relationProperty = _contextEntityProvider.GetContextEntity(relationship.DependentType);
            var attr = relationProperty.Attributes.SingleOrDefault(a => a.Is(field));
            if (attr == null)
                throw new JsonApiException(400, $"'{relationship.DependentType.Name}' does not contain '{field}'.");

            if (!_selectedRelationshipFields.TryGetValue(relationship, out var registeredFields))
                _selectedRelationshipFields.Add(relationship, registeredFields = new List<AttrAttribute>());
            registeredFields.Add(attr);
        }

        /// <summary>
        /// Registers field selection queries of the form articles?fields=title
        /// </summary>
        private void RegisterRequestResourceField(string field)
        {
            var attr = _requestResource.Attributes.SingleOrDefault(a => a.Is(field));
            if (attr == null)
                throw new JsonApiException(400, $"'{_requestResource.EntityName}' does not contain '{field}'.");

            (_selectedFields = _selectedFields ?? new List<AttrAttribute>()).Add(attr);
        }
    }
}
