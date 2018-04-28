namespace JsonApiDotNetCore.Models
{
    public class HasManyAttribute : RelationshipAttribute
    {
        public HasManyAttribute(string publicName, Link documentLinks = Link.All, bool canInclude = true)
        : base(publicName, documentLinks, canInclude)
        { }

        public override void SetValue(object entity, object newValue)
        {
            var propertyInfo = entity
                .GetType()
                .GetProperty(InternalRelationshipName);
            
            propertyInfo.SetValue(entity, newValue);
        }
    }
}
