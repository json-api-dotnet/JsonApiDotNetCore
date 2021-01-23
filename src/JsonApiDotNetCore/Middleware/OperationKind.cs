namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Lists the functional operation kinds from an atomic:operations request.
    /// </summary>
    public enum OperationKind
    {
        CreateResource,
        UpdateResource,
        DeleteResource,
        SetRelationship,
        AddToRelationship,
        RemoveFromRelationship
    }
}
