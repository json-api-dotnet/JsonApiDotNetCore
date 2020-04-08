using System.Text;

namespace JsonApiDotNetCore.Graph
{
    /// <summary>
    /// Uses kebab-case as formatting options in the route and request/response body.
    /// </summary>
    /// <example>
    /// <code>
    /// _default.FormatResourceName(typeof(TodoItem)).Dump(); 
    /// // > "todoItems"
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
    /// // > "todoItems"
    ///
    /// _default.ApplyCasingConvention("TodoItem"); 
    /// // > "todo-item"
    /// </code>
    /// </example>
    public sealed class KebabCaseFormatter : BaseResourceNameFormatter
    {
        /// <inheritdoc/>
        public override string ApplyCasingConvention(string properName)
        {
            if (properName.Length == 0)
            {
                return properName;
            }

            var chars = properName.ToCharArray();
            var builder = new StringBuilder();

            for (var i = 0; i < chars.Length; i++)
            {
                if (char.IsUpper(chars[i]))
                {
                    if (i > 0)
                    {
                        builder.Append('-');
                    }

                    builder.Append(char.ToLower(chars[i]));
                }
                else
                {
                    builder.Append(chars[i]);
                }
            }

            return builder.ToString();
        }
    }
}
