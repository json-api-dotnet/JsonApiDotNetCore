using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceGeneratorTests
{
    internal sealed class SourceCodeBuilder
    {
        private readonly HashSet<string> _namespaceImports = new();
        private string? _namespace;
        private string? _code;

        public string Build()
        {
            StringBuilder builder = new();

            if (_namespaceImports.Any())
            {
                foreach (string namespaceImport in _namespaceImports)
                {
                    builder.AppendLine($"using {namespaceImport};");
                }

                builder.AppendLine();
            }

            if (_namespace != null)
            {
                builder.AppendLine($"namespace {_namespace}");
                builder.AppendLine("{");
            }

            if (_code != null)
            {
                builder.Append(_code);
            }

            if (_namespace != null)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public SourceCodeBuilder WithNamespaceImportFor(Type type)
        {
            _namespaceImports.Add(type.Namespace!);
            return this;
        }

        public SourceCodeBuilder InNamespace(string @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        public SourceCodeBuilder WithCode(string code)
        {
            _code = code;
            return this;
        }
    }
}
