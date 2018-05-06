using System;
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
                ? GetNamespaceFromPath(r.Path, entityName)
                : $"{r.Scheme}://{r.Host}{GetNamespaceFromPath(r.Path, entityName)}";
        }

        internal static string GetNamespaceFromPath(string path, string entityName)
        {
            var entityNameSpan = entityName.AsSpan();
            var pathSpan = path.AsSpan();
            const char delimiter = '/';
            for (var i = 0; i < pathSpan.Length; i++)
            {
                if(pathSpan[i].Equals(delimiter))
                {
                    var nextPosition = i + 1;
                    if(pathSpan.Length > i + entityNameSpan.Length)
                    {
                        var possiblePathSegment = pathSpan.Slice(nextPosition, entityNameSpan.Length);
                        if (entityNameSpan.SequenceEqual(possiblePathSegment)) 
                        {
                            // check to see if it's the last position in the string
                            //   or if the next character is a /
                            var lastCharacterPosition = nextPosition + entityNameSpan.Length;

                            if(lastCharacterPosition == pathSpan.Length || pathSpan.Length >= lastCharacterPosition + 2 && pathSpan[lastCharacterPosition].Equals(delimiter))
                            {
                                return pathSpan.Slice(0, i).ToString();
                            }
                        }
                    }
                }
            }

            return string.Empty;
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
