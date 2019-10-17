using str = JsonApiDotNetCore.Extensions.StringExtensions;

namespace JsonApiDotNetCore.Graph
{
    public class KebabCaseFormatter : BaseResourceNameFormatter
    {
        /// <summary>
        /// Aoplies the desired casing convention to the internal string.
        /// This is generally applied to the type name after pluralization.
        /// </summary>
        ///
        /// <example>
        /// <code>
        /// _default.ApplyCasingConvention("TodoItems"); 
        /// // > "todo-items"
        ///
        /// _default.ApplyCasingConvention("TodoItem"); 
        /// // > "todo-item"
        /// </code>
        /// </example>
        public override string ApplyCasingConvention(string properName) => str.Dasherize(properName);
    }
}
