using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
    public interface IDbContextResolver
    {
        DbContext GetContext();
    }
}
