using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Exceptions;
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
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Fields);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            var isRelated = parameterName.StartsWith("fields[") && parameterName.EndsWith("]");
            return parameterName == "fields" || isRelated;
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            // expected: articles?fields=prop1,prop2
            //           articles?fields[articles]=prop1,prop2  <-- this form in invalid UNLESS "articles" is actually a relationship on Article
            //           articles?fields[relationship]=prop1,prop2
            EnsureNoNestedResourceRoute(parameterName);
            var fields = new List<string> { nameof(Identifiable.Id) };
            fields.AddRange(((string)parameterValue).Split(QueryConstants.COMMA));

            var keySplit = parameterName.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET);

            if (keySplit.Length == 1)
            {   // input format: fields=prop1,prop2
                foreach (var field in fields)
                    RegisterRequestResourceField(field, parameterName);
            }
            else
            {  // input format: fields[articles]=prop1,prop2
                string navigation = keySplit[1];
                // it is possible that the request resource has a relationship
                // that is equal to the resource name, like with self-referencing data types (eg directory structures)
                // if not, no longer support this type of sparse field selection.
                if (navigation == _requestResource.ResourceName && !_requestResource.Relationships.Any(a => a.Is(navigation)))
                {
                    throw new InvalidQueryStringParameterException(parameterName,
                        "Square bracket notation in 'filter' is now reserved for relationships only. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/555#issuecomment-543100865 for details.",
                        $"Use '?fields=...' instead of '?fields[{navigation}]=...'.");
                }

                if (navigation.Contains(QueryConstants.DOT))
                {
                    throw new InvalidQueryStringParameterException(parameterName,
                        "Deeply nested sparse field selection is currently not supported.",
                        $"Parameter fields[{navigation}] is currently not supported.");
                }

                var relationship = _requestResource.Relationships.SingleOrDefault(a => a.Is(navigation));
                if (relationship == null)
                {
                    throw new InvalidQueryStringParameterException(parameterName, "Sparse field navigation path refers to an invalid relationship.",
                        $"'{navigation}' in 'fields[{navigation}]' is not a valid relationship of {_requestResource.ResourceName}.");
                }

                foreach (var field in fields)
                    RegisterRelatedResourceField(field, relationship, parameterName);
            }
        }

        /// <summary>
        /// Registers field selection queries of the form articles?fields[author]=firstName
        /// </summary>
        private void RegisterRelatedResourceField(string field, RelationshipAttribute relationship, string parameterName)
        {
            var relationProperty = _resourceGraph.GetResourceContext(relationship.RightType);
            var attr = relationProperty.Attributes.SingleOrDefault(a => a.Is(field));
            if (attr == null)
            {
                // TODO: Add unit test for this error, once the nesting limitation is removed and this code becomes reachable again.

                throw new InvalidQueryStringParameterException(parameterName, "Sparse field navigation path refers to an invalid related field.",
                    $"Related resource '{relationship.RightType.Name}' does not contain an attribute named '{field}'.");
            }

            if (!_selectedRelationshipFields.TryGetValue(relationship, out var registeredFields))
                _selectedRelationshipFields.Add(relationship, registeredFields = new List<AttrAttribute>());
            registeredFields.Add(attr);
        }

        /// <summary>
        /// Registers field selection queries of the form articles?fields=title
        /// </summary>
        private void RegisterRequestResourceField(string field, string parameterName)
        {
            var attr = _requestResource.Attributes.SingleOrDefault(a => a.Is(field));
            if (attr == null)
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "The specified field does not exist on the requested resource.",
                    $"The field '{field}' does not exist on resource '{_requestResource.ResourceName}'.");
            }

            (_selectedFields ??= new List<AttrAttribute>()).Add(attr);
        }
    }
}
