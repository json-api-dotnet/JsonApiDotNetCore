using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    public interface IDbContextResolver
    {
        DbContext GetContext();
    }
}
