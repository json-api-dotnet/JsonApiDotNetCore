namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Lists the functional operations from an atomic:operations request.
    /// See also <see cref="OperationKindExtensions"/>.
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
