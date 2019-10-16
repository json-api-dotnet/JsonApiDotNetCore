using JsonApiDotNetCore.Models;
using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Services
{
    public class UpdateValueHelper<TResource> : IUpdateValueHelper<TResource> where TResource : IIdentifiable
    {
        private readonly IFieldsExplorer _explorer;
        private readonly ITargetedFields _targetedFields;

        public UpdateValueHelper(IFieldsExplorer explorer, ITargetedFields targetedFields)
        {
            _explorer = explorer;
            _targetedFields = targetedFields;
        }

        public void MarkUpdated(Expression<Func<TResource, dynamic>> selector)
        {
            var fields = _explorer.GetFields(selector);

            foreach (var field in fields)
            {
                if (field is AttrAttribute attribute)
                    _targetedFields.Attributes.Add(attribute);
                else if (field is RelationshipAttribute relationship)
                    _targetedFields.Relationships.Add(relationship);
            }

        }
    }
}
