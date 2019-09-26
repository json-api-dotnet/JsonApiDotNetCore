namespace JsonApiDotNetCore.Models
{
    public interface IResourceField
    {
        string ExposedInternalMemberName { get; }
    }
}