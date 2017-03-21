namespace JsonApiDotNetCoreExampleTests.Services
{
    public interface IAuthorizationService
    {
        int CurrentUserId { get; set; }
    }
}
