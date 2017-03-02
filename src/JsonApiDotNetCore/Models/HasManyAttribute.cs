namespace JsonApiDotNetCore.Models
{
    public class HasManyAttribute : RelationshipAttribute
    {
        public HasManyAttribute(string publicName)
        : base(publicName)
        {
            PublicRelationshipName = publicName;
        }
    }
}
