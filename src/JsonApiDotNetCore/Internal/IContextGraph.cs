namespace JsonApiDotNetCore.Internal
{
    public interface IContextGraph
    {
        object GetRelationship<TParent>(TParent entity, string relationshipName);
        string GetRelationshipName<TParent>(string relationshipName);
        ContextEntity GetContextEntity(string dbSetName);
    }
}
