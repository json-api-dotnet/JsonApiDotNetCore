using System.Collections.Generic;
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

        public SparseFieldsService()
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
        //public override Parse(AttrAttribute selected, RelationshipAttribute relationship = null)
        public override void Parse(string value)
        {
            if (relationship == null)
            {
                _selectedFields = _selectedFields ?? new List<AttrAttribute>();
                _selectedFields.Add(selected);
            } else
            {
            if (!_selectedRelationshipFields.TryGetValue(relationship, out var fields))
                _selectedRelationshipFields.Add(relationship, fields = new List<AttrAttribute>());

            fields.Add(selected);
            }
        }
    }
}
