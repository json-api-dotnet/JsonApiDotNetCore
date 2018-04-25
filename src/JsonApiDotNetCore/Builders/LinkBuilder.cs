using System;
using System.Text;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Builders
{
    public class LinkBuilder
    {
        private readonly IJsonApiContext _context;

        public LinkBuilder(IJsonApiContext context)
        {
            _context = context;
        }

        public string GetBasePath(HttpContext context, string entityName)
        {
            var r = context.Request;
            return (_context.Options.RelativeLinks)
                ? $"{GetNamespaceFromPath(r.Path, entityName)}"
                : $"{r.Scheme}://{r.Host}{GetNamespaceFromPath(r.Path, entityName)}";
        }

        private static string GetNamespaceFromPath(string path, string entityName)
        {
            var sb = new StringBuilder();
            var entityNameSpan = entityName.AsSpan();
            var subSpans = path.SpanSplit('/');
            for (var i = 1; i < subSpans.Count; i++)
            {
                var span = subSpans[i];
                if (entityNameSpan.SequenceEqual(span)) break;
                sb.Append($"/{span.ToString()}");
            }
            return sb.ToString();
        }

        public string GetSelfRelationLink(string parent, string parentId, string child)
        {
            return $"{_context.BasePath}/{parent}/{parentId}/relationships/{child}";
        }

        public string GetRelatedRelationLink(string parent, string parentId, string child)
        {
            return $"{_context.BasePath}/{parent}/{parentId}/{child}";
        }

        public string GetPageLink(int pageOffset, int pageSize)
        {
            return $"{_context.BasePath}/{_context.RequestEntity.EntityName}?page[size]={pageSize}&page[number]={pageOffset}";
        }
    }
}
