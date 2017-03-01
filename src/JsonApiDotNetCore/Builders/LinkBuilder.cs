using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Builders
{
    public class LinkBuilder
    {
        IJsonApiContext _context;

        public LinkBuilder(IJsonApiContext context)
        {
            _context = context;   
        }

        public string GetBasePath(HttpContext context, string entityName)
        {
            var r = context.Request;
            return $"{r.Scheme}://{r.Host}{GetNamespaceFromPath(r.Path, entityName)}";
        }

        private string GetNamespaceFromPath(string path, string entityName)
        {
            var nSpace = string.Empty;
            var segments = path.Split('/');
            
            for(var i = 1; i < segments.Length; i++)
            {
                if(segments[i].ToLower() == entityName.Dasherize()) 
                    break;

                nSpace += $"/{segments[i].Dasherize()}";
            }
                
            return nSpace;
        }

        public string GetSelfRelationLink(string parent, string parentId, string child)
        {
            return $"{_context.BasePath}/{parent.Dasherize()}/{parentId}/relationships/{child.Dasherize()}";
        }

        public string GetRelatedRelationLink(string parent, string parentId, string child)
        {
            return $"{_context.BasePath}/{parent.Dasherize()}/{parentId}/{child.Dasherize()}";
        }

        public string GetPageLink(int pageOffset, int pageSize)
        {
            return $"{_context.BasePath}/{_context.RequestEntity.EntityName.Dasherize()}?page[size]={pageSize}&page[number]={pageOffset}";
        }
    }
}
