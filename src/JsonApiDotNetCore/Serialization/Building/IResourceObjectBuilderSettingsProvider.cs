namespace JsonApiDotNetCore.Serialization.Building
{
    /// <summary>
    /// Service that provides the server serializer with <see cref="ResourceObjectBuilderSettings" />.
    /// </summary>
    public interface IResourceObjectBuilderSettingsProvider
    {
        /// <summary>
        /// Gets the behavior for the serializer it is injected in.
        /// </summary>
        ResourceObjectBuilderSettings Get();
    }
}
