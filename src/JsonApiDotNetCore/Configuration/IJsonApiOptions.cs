using System;
using System.Collections.Generic;
using System.Text;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Configuration
{
    public interface IJsonApiOptions
    {
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
        bool ValidateModelState { get; }
        bool AllowClientGeneratedIds { get; }
        JsonSerializerSettings SerializerSettings { get; }
        bool EnableOperations { get; set; }
        Link DefaultRelationshipLinks { get; set; }
        NullAttributeResponseBehavior NullAttributeResponseBehavior { get; set; }
        bool RelativeLinks { get; set; }
        IResourceGraph ResourceGraph { get; set; }
        bool AllowCustomQueryParameters { get; set; }
        string Namespace { get; set; }
    }
}
