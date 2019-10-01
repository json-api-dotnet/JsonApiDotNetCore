using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCore.Configuration
{
    public interface IJsonApiOptions : ILinksConfiguration, ISerializerOptions
    {
        /// <summary>
        /// Whether or not database values should be included by default
        /// for resource hooks. Ignored if EnableResourceHooks is set false.
        /// 
        /// Defaults to <see langword="false"/>.
        /// </summary>
        bool LoaDatabaseValues { get; set; }
        /// <summary>
        /// Whether or not the total-record count should be included in all document
        /// level meta objects.
        /// Defaults to false.
        /// </summary>
        /// <example>
        /// <code>options.IncludeTotalRecordCount = true;</code>
        /// </example>
        bool IncludeTotalRecordCount { get; set; }
        int DefaultPageSize { get; }
        bool ValidateModelState { get; }
        bool AllowClientGeneratedIds { get; }
        bool EnableOperations { get; set; }
        IResourceGraph ResourceGraph { get; set; }
        bool AllowCustomQueryParameters { get; set; }
        string Namespace { get; set; }
    }

    public interface ISerializerOptions
    {
        NullAttributeResponseBehavior NullAttributeResponseBehavior { get; set; }
        DefaultAttributeResponseBehavior DefaultAttributeResponseBehavior { get; set; }
    }
}
