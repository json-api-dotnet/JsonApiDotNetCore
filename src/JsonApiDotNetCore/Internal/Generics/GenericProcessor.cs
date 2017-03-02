using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            var relationshipType = relationship.Type;

            // TODO: replace with relationship.IsMany
            if(relationship.Type.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                var entities = _context.GetDbSet<T>().Where(x => relationshipIds.Contains(x.Id.ToString())).ToList();
                relationship.SetValue(parent, entities);
            }
            else
            {
                var entity = _context.GetDbSet<T>().SingleOrDefault(x => relationshipIds.First() == x.Id.ToString());
                relationship.SetValue(parent, entity);
            }            

            await _context.SaveChangesAsync();
        }
    }
}
