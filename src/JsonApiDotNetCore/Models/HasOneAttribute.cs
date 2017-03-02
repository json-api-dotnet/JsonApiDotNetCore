namespace JsonApiDotNetCore.Models
{
    public class HasOneAttribute : RelationshipAttribute
    {
        public HasOneAttribute(string publicName)
        : base(publicName)
        {
            PublicRelationshipName = publicName;
        }
    }
}
