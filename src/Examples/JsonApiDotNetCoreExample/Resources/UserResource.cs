using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCoreExample.Resources
{
    public class UserResource : ResourceDefinition<User>
    {
        public UserResource(IResourceGraph resourceGraph) : base(resourceGraph)
        {
            HideFields(u => u.Password);
        }

        public override QueryFilters GetQueryFilters()
        {
          return new QueryFilters
          {
            { "first-character", (users, queryFilter) => FirstCharacterFilter(users, queryFilter) }
          };
        }

        private IQueryable<User> FirstCharacterFilter(IQueryable<User> users, FilterQuery filterQuery)
        {
            switch (filterQuery.Operation)
            {
                // need to cast to list first because getting the first
                // char in a string is apparently not something LINQ can translate
                // to a query.
                case "lt":
                    return users.ToList().Where(u => u.Username.First() < filterQuery.Value[0]).AsQueryable();
                default:
                    return users.ToList().Where(u => u.Username.First() == filterQuery.Value[0]).AsQueryable();
            }
        }
    }
}
