using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a sparse fieldset, resulting from text such as: firstName,lastName
    /// </summary>
    public class SparseFieldSetExpression : QueryExpression
    {
        public IReadOnlyCollection<AttrAttribute> Attributes { get; }

        public SparseFieldSetExpression(IReadOnlyCollection<AttrAttribute> attributes)
        {
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));

            if (!attributes.Any())
            {
                throw new ArgumentException("Must have one or more attributes.", nameof(attributes));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSparseFieldSet(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Attributes.Select(child => child.PublicName));
        }
    }
}
