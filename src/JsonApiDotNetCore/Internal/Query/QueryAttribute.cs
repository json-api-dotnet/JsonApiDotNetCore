namespace JsonApiDotNetCore.Internal.Query
{
    public class QueryAttribute
    {
        public QueryAttribute(string attribute)
        {
            var attributes = attribute.Split('.');
            if (attributes.Length > 1)
            {
                RelationshipAttribute = attributes[0];
                Attribute = attributes[1];
                IsAttributeOfRelationship = true;
            }
            else
            {
                Attribute = attribute;
                IsAttributeOfRelationship = false;
            }

        }
        
        public string Attribute { get; }
        public string RelationshipAttribute { get; }
        public bool IsAttributeOfRelationship { get; }
    }
}
