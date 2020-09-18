namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    public interface ISoftDeletable
    {
        bool IsSoftDeleted { get; set; }
    }
}
