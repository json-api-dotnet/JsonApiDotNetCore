using System;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Configuration
{
    public interface IJsonApiOptions : ILinksConfiguration, ISerializerOptions
    {
        /// <summary>
        /// Whether or not <see cref="Exception"/> stack traces should be serialized in <see cref="ErrorMeta"/> objects.
        /// </summary>
        bool IncludeExceptionStackTraceInErrors { get; set; }

        /// <summary>
        /// Whether or not database values should be included by default
        /// for resource hooks. Ignored if EnableResourceHooks is set false.
        /// 
        /// Defaults to <see langword="false"/>.
        /// </summary>
        bool LoadDatabaseValues { get; set; }
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
        int? MaximumPageSize { get; }
        int? MaximumPageNumber { get; }
        bool ValidateModelState { get; }
        bool AllowClientGeneratedIds { get; }
        bool AllowCustomQueryStringParameters { get; set; }
        string Namespace { get; set; }
    }

    public interface ISerializerOptions
    {
        NullAttributeResponseBehavior NullAttributeResponseBehavior { get; set; }
        DefaultAttributeResponseBehavior DefaultAttributeResponseBehavior { get; set; }
    }
}
