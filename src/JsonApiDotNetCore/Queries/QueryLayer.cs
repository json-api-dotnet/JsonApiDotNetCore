using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// A nested data structure that contains <see cref="QueryExpression" /> constraints per resource type.
    /// </summary>
    [PublicAPI]
    public sealed class QueryLayer
    {
        public ResourceContext ResourceContext { get; }

        public IncludeExpression Include { get; set; }
        public FilterExpression Filter { get; set; }
        public SortExpression Sort { get; set; }
        public PaginationExpression Pagination { get; set; }
        public IDictionary<ResourceFieldAttribute, QueryLayer> Projection { get; set; }

        public QueryLayer(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            ResourceContext = resourceContext;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            var writer = new IndentingStringWriter(builder);
            WriteLayer(writer, this);

            return builder.ToString();
        }

        private static void WriteLayer(IndentingStringWriter writer, QueryLayer layer, string prefix = null)
        {
            writer.WriteLine(prefix + nameof(QueryLayer) + "<" + layer.ResourceContext.ResourceType.Name + ">");

            using (writer.Indent())
            {
                if (layer.Include != null)
                {
                    writer.WriteLine($"{nameof(Include)}: {layer.Include}");
                }

                if (layer.Filter != null)
                {
                    writer.WriteLine($"{nameof(Filter)}: {layer.Filter}");
                }

                if (layer.Sort != null)
                {
                    writer.WriteLine($"{nameof(Sort)}: {layer.Sort}");
                }

                if (layer.Pagination != null)
                {
                    writer.WriteLine($"{nameof(Pagination)}: {layer.Pagination}");
                }

                if (!layer.Projection.IsNullOrEmpty())
                {
                    writer.WriteLine(nameof(Projection));

                    using (writer.Indent())
                    {
                        foreach ((ResourceFieldAttribute field, QueryLayer nextLayer) in layer.Projection)
                        {
                            if (nextLayer == null)
                            {
                                writer.WriteLine(field.ToString());
                            }
                            else
                            {
                                WriteLayer(writer, nextLayer, field.PublicName + ": ");
                            }
                        }
                    }
                }
            }
        }

        private sealed class IndentingStringWriter : IDisposable
        {
            private readonly StringBuilder _builder;
            private int _indentDepth;

            public IndentingStringWriter(StringBuilder builder)
            {
                _builder = builder;
            }

            public void WriteLine(string line)
            {
                if (_indentDepth > 0)
                {
                    _builder.Append(new string(' ', _indentDepth * 2));
                }

                _builder.AppendLine(line);
            }

            public IndentingStringWriter Indent()
            {
                WriteLine("{");
                _indentDepth++;
                return this;
            }

            public void Dispose()
            {
                if (_indentDepth > 0)
                {
                    _indentDepth--;
                    WriteLine("}");
                }
            }
        }
    }
}
