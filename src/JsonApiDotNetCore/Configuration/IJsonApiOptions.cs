using System;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Configuration
{
    public interface IJsonApiOptions : ILinksConfiguration
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

        /// <summary>
        /// Determines whether the <see cref="JsonSerializerSettings.NullValueHandling"/> serialization setting can be overridden by using a query string parameter.
        /// </summary>
        bool AllowQueryStringOverrideForSerializerNullValueHandling { get; set; }

        /// <summary>
        /// Determines whether the <see cref="JsonSerializerSettings.DefaultValueHandling"/> serialization setting can be overridden by using a query string parameter.
        /// </summary>
        bool AllowQueryStringOverrideForSerializerDefaultValueHandling { get; set; }

        /// <summary>
        /// Specifies the settings that are used by the <see cref="JsonSerializer"/>.
        /// Note that at some places a few settings are ignored, to ensure json:api spec compliance.
        /// <example>
        /// The next example changes the casing convention to kebab casing.
        /// <code><![CDATA[
        /// options.SerializerSettings.ContractResolver = new DefaultContractResolver
        /// {
        ///     NamingStrategy = new KebabCaseNamingStrategy()
        /// };
        /// ]]></code>
        /// </example>
        /// </summary>
        JsonSerializerSettings SerializerSettings { get; }

        internal DefaultContractResolver SerializerContractResolver => (DefaultContractResolver)SerializerSettings.ContractResolver;
    }
}
