using System.Collections;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IJsonApiSerializer
    {
        string Serialize(object content);
    }
}