using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    public class GenericProcessor<T> : IGenericProcessor where T : class, IIdentifiable
    {
        private readonly DbContext _context;
        public GenericProcessor(DbContext context)
        {
            _context = context;
        }

        public async Task UpdateRelationshipsAsync(object parent, Relationship relationship, IEnumerable<string> relationshipIds)
        {
            var relationshipType = relationship.BaseType;

            var entities = _context.GetDbSet<T>().Where(x => relationshipIds.Contains(x.Id.ToString())).ToList();
            relationship.SetValue(parent, entities);

            await _context.SaveChangesAsync();
        }
    }
}
