using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class SparseFieldsService : QueryParameterService, ISparseFieldsService
    {
        /// <summary>
        /// The selected fields for the primary resource of this request.
        /// </summary>
        private readonly List<AttrAttribute> _selectedFields = new List<AttrAttribute>();

        /// <summary>
        /// The selected field for any included relationships
        /// </summary>
        private readonly Dictionary<RelationshipAttribute, List<AttrAttribute>> _selectedRelationshipFields = new Dictionary<RelationshipAttribute, List<AttrAttribute>>();

        public SparseFieldsService(IResourceGraph resourceGraph, ICurrentRequest currentRequest) 
            : base(resourceGraph, currentRequest)
        {
        }

        /// <inheritdoc/>
        public List<AttrAttribute> Get(RelationshipAttribute relationship = null)
        {
            if (relationship == null)
                return _selectedFields;

            return _selectedRelationshipFields.TryGetValue(relationship, out var fields)
                ? fields
                : new List<AttrAttribute>();
        }

        public ISet<string> GetAll()
        {
            var properties = new HashSet<string>();
            properties.AddRange(_selectedFields.Select(x => x.Property.Name));

            foreach (var pair in _selectedRelationshipFields)
            {
                string pathPrefix = pair.Key.RelationshipPath + ".";
                properties.AddRange(pair.Value.Select(x => pathPrefix + x.Property.Name));
            }

            return properties;
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

            HashSet<string> fields = new HashSet<string>();
            fields.Add(nameof(Identifiable.Id).ToLowerInvariant());
            fields.AddRange(((string) parameterValue).Split(QueryConstants.COMMA));

            var keySplit = parameterName.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET);

            if (keySplit.Length == 1)
            {
                // input format: fields=prop1,prop2
                RegisterRequestResourceFields(fields, parameterName);
            }
            else
            {  // input format: fields[articles]=prop1,prop2
                string navigation = keySplit[1];
                // it is possible that the request resource has a relationship
                // that is equal to the resource name, like with self-referencing data types (eg directory structures)
                // if not, no longer support this type of sparse field selection.
                if (navigation == _requestResource.ResourceName && !_requestResource.Relationships.Any(a => navigation == a.PublicName))
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

                var relationship = _requestResource.Relationships.SingleOrDefault(a => navigation == a.PublicName);
                if (relationship == null)
                {
                    throw new InvalidQueryStringParameterException(parameterName, "Sparse field navigation path refers to an invalid relationship.",
                        $"'{navigation}' in 'fields[{navigation}]' is not a valid relationship of {_requestResource.ResourceName}.");
                }

                RegisterRelatedResourceFields(fields, relationship, parameterName);
            }
        }

        /// <summary>
        /// Registers field selection of the form articles?fields[author]=firstName,lastName
        /// </summary>
        private void RegisterRelatedResourceFields(IEnumerable<string> fields, RelationshipAttribute relationship, string parameterName)
        {
            var selectedFields = new List<AttrAttribute>();

            foreach (var field in fields)
            {
                var relationProperty = _resourceGraph.GetResourceContext(relationship.RightType);
                var attr = relationProperty.Attributes.SingleOrDefault(a => field == a.PublicName);
                if (attr == null)
                {
                    throw new InvalidQueryStringParameterException(parameterName,
                        "The specified field does not exist on the requested related resource.",
                        $"The field '{field}' does not exist on related resource '{relationship.PublicName}' of type '{relationProperty.ResourceName}'.");
                }

                if (attr.Property.SetMethod == null)
                {
                    // A read-only property was selected. Its value likely depends on another property, so include all related fields.
                    return;
                }

                selectedFields.Add(attr);
            }

            if (!_selectedRelationshipFields.TryGetValue(relationship, out var registeredFields))
            {
                _selectedRelationshipFields.Add(relationship, registeredFields = new List<AttrAttribute>());
            }
            registeredFields.AddRange(selectedFields);
        }

        /// <summary>
        /// Registers field selection of the form articles?fields=title,description
        /// </summary>
        private void RegisterRequestResourceFields(IEnumerable<string> fields, string parameterName)
        {
            var selectedFields = new List<AttrAttribute>();

            foreach (var field in fields)
            {
                var attr = _requestResource.Attributes.SingleOrDefault(a => field == a.PublicName);
                if (attr == null)
                {
                    throw new InvalidQueryStringParameterException(parameterName,
                        "The specified field does not exist on the requested resource.",
                        $"The field '{field}' does not exist on resource '{_requestResource.ResourceName}'.");
                }

                if (attr.Property.SetMethod == null)
                {
                    // A read-only property was selected. Its value likely depends on another property, so include all resource fields.
                    return;
                }

                selectedFields.Add(attr);
            }

            _selectedFields.AddRange(selectedFields);
        }
    }
}
