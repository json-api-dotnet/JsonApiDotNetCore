using str = JsonApiDotNetCore.Extensions.StringExtensions;

namespace JsonApiDotNetCore.Graph
{
    public class CamelCaseFormatter: BaseResourceNameFormatter
    {
        /// <summary>
        /// Aoplies the desired casing convention to the internal string.
        /// This is generally applied to the type name after pluralization.
        /// </summary>
        ///
        /// <example>
        /// <code>
        /// _default.ApplyCasingConvention("TodoItems"); 
        /// // > "todoItems"
        ///Came
        /// _default.ApplyCasingConvention("TodoItem"); 
        /// // > "todoItem"
        /// </code>
        /// </example>
        public override string ApplyCasingConvention(string properName) => str.Camelize(properName);
    }
}

