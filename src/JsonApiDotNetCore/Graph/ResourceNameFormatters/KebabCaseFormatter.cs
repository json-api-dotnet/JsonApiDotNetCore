using str = JsonApiDotNetCore.Extensions.StringExtensions;

namespace JsonApiDotNetCore.Graph
{
    /// <summary>
    /// Uses kebab-case as formatting options in the route and request/response body.
    /// </summary>
    /// <example>
    /// <code>
    /// _default.FormatResourceName(typeof(TodoItem)).Dump(); 
    /// // > "todo-items"
    /// </code>
    /// </example>
    /// <example>
    /// Given the following property:
    /// <code>
    /// public string CompoundProperty { get; set; }
    /// </code>
    /// The public attribute will be formatted like so:
    /// <code>
    /// _default.FormatPropertyName(compoundProperty).Dump(); 
    /// // > "compound-property"
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// _default.ApplyCasingConvention("TodoItems"); 
    /// // > "todo-items"
    ///
    /// _default.ApplyCasingConvention("TodoItem"); 
    /// // > "todo-item"
    /// </code>
    /// </example>
    public class KebabCaseFormatter : BaseResourceNameFormatter
    {
        /// <inheritdoc/>
        public override string ApplyCasingConvention(string properName) => str.Dasherize(properName);
    }
}
