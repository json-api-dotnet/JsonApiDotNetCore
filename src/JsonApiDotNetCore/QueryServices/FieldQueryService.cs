using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public class FieldsQueryService : IFieldsQueryService, IInternalFieldsQueryService
    {
        private List<AttrAttribute> _selectedFields;
        private readonly Dictionary<RelationshipAttribute, List<AttrAttribute>> _selectedRelationshipFields;

        public FieldsQueryService()
        {
            _selectedFields = new List<AttrAttribute>();
            _selectedRelationshipFields = new Dictionary<RelationshipAttribute, List<AttrAttribute>>();
        }

        public List<AttrAttribute> Get(RelationshipAttribute relationship = null)
        {
            if (relationship == null)
                return _selectedFields;

            _selectedRelationshipFields.TryGetValue(relationship, out var fields);
            return fields;
        }

        public void Register(AttrAttribute selected, RelationshipAttribute relationship = null)
        {
            if (relationship == null)
            {
                _selectedFields = _selectedFields ?? new List<AttrAttribute>();
                _selectedFields.Add(selected);
            }

            if (!_selectedRelationshipFields.TryGetValue(relationship, out var fields))
                _selectedRelationshipFields.Add(relationship, fields = new List<AttrAttribute>());

            fields.Add(selected);
        }
    }
}
