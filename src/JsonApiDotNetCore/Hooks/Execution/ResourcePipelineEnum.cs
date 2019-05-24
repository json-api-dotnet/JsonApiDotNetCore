namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// An enum that represents the initiator of a resource hook. Eg, when BeforeCreate()
    /// is called from EntityResourceService.GetAsync(TId id), it will be called
    /// with parameter pipeline = ResourceAction.GetSingle.
    /// </summary>
    public enum ResourcePipeline
    {
        None,
        Read,
        ReadSingle,
        ReadRelationship,
        Create,
        Patch,
        PatchRelationship,
        Delete,
        BulkCreate,
        BulkPatch,
        BulkDelete
    }
}