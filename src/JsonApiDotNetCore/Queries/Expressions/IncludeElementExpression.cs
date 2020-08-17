using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents an element in <see cref="IncludeExpression"/>.
    /// </summary>
    public class IncludeElementExpression : QueryExpression
    {
        public RelationshipAttribute Relationship { get; }
        public IReadOnlyCollection<IncludeElementExpression> Children { get; }

        public IncludeElementExpression(RelationshipAttribute relationship)
            : this(relationship, Array.Empty<IncludeElementExpression>())
        {
        }

        public IncludeElementExpression(RelationshipAttribute relationship, IReadOnlyCollection<IncludeElementExpression> children)
        {
            Relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
            Children = children ?? throw new ArgumentNullException(nameof(children));
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitIncludeElement(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Relationship);

            if (Children.Any())
            {
                builder.Append('{');
                builder.Append(string.Join(",", Children.Select(child => child.ToString())));
                builder.Append('}');
            }
            
            return builder.ToString();
        }
    }
}
