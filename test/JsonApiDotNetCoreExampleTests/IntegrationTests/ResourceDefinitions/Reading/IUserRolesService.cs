namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    public interface IUserRolesService
    {
        bool AllowIncludeOwner { get; }
    }
}
