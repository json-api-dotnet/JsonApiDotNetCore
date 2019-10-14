using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public interface IQueryParser
    {
        void Parse(IQueryCollection query, DisableQueryAttribute disabledQuery);
    }
}
