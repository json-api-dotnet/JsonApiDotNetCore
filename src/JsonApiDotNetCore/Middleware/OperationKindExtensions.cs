namespace JsonApiDotNetCore.Middleware
{
    public static class OperationKindExtensions
    {
        public static bool IsRelationship(this OperationKind kind)
        {
            return IsRelationship((OperationKind?)kind);
        }

        public static bool IsRelationship(this OperationKind? kind)
        {
            return kind == OperationKind.SetRelationship || kind == OperationKind.AddToRelationship ||
                   kind == OperationKind.RemoveFromRelationship;
        }
    }
}
