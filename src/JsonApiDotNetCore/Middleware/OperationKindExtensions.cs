namespace JsonApiDotNetCore.Middleware
{
    public static class OperationKindExtensions
    {
        public static bool IsRelationship(this OperationKind kind)
        {
            return kind == OperationKind.SetRelationship || kind == OperationKind.AddToRelationship ||
                   kind == OperationKind.RemoveFromRelationship;
        }

        public static bool IsResource(this OperationKind kind)
        {
            return kind == OperationKind.CreateResource || kind == OperationKind.UpdateResource ||
                   kind == OperationKind.DeleteResource;
        }
    }
}
