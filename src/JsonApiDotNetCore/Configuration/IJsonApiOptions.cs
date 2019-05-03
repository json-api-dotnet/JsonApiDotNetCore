using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Configuration
{
    public interface IJsonApiOptions
    {
        /// <summary>
        /// Whether or not the total-record count should be included in all document
        /// level meta objects.
        /// Defaults to false.
        /// </summary>
        /// <example>
        /// <code>options.IncludeTotalRecordCount = true;</code>
        /// </example>
        bool IncludeTotalRecordCount { get; set; }
    }
}
