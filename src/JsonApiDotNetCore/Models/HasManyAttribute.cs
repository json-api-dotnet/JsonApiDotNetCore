using System.Reflection;

namespace JsonApiDotNetCore.Models
{
    public class HasManyAttribute : RelationshipAttribute
    {
        public HasManyAttribute(string publicName)
        : base(publicName)
        {
            PublicRelationshipName = publicName;
        }

        public override void SetValue(object entity, object newValue)
        {
            var propertyInfo = entity
                .GetType()
                .GetProperty(InternalRelationshipName);
            
            propertyInfo.SetValue(entity, newValue);        
        }
    }
}
