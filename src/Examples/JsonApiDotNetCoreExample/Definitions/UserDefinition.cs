using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public class UserDefinition : ResourceDefinition<User>
    {
        public UserDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
            HideFields(u => u.Password);
        }

        public override QueryFilters GetQueryFilters()
        {
          return new QueryFilters
          {
            { "firstCharacter", FirstCharacterFilter }
          };
        }

        private IQueryable<User> FirstCharacterFilter(IQueryable<User> users, FilterQuery filterQuery)
        {
            switch (filterQuery.Operation)
            {
                // In EF core >= 3.0 we need to explicitly evaluate the query first. This could probably be translated
                // into a query by building expression trees.
                case "lt":
                    return users.ToList().Where(u => u.Username.First() < filterQuery.Value[0]).AsQueryable();
                default:
                    return users.ToList().Where(u => u.Username.First() == filterQuery.Value[0]).AsQueryable();
            }
        }
    }
}
