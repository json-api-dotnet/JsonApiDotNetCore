namespace JsonApiDotNetCore.Serialization.Objects
{
    public interface IResourceIdentity
    {
        public string? Type { get; }
        public string? Id { get; }
        public string? Lid { get; }
    }
}
