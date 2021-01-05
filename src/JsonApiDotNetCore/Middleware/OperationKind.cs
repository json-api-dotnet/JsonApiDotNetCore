namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Lists the functional operations from an atomic:operations request.
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
