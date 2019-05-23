using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Builders
{
    public interface ILinkBuilder
    {
        string GetPageLink(int pageOffset, int pageSize);
        string GetRelatedRelationLink(string parent, string parentId, string child);
        string GetSelfRelationLink(string parent, string parentId, string child);
    }
}
