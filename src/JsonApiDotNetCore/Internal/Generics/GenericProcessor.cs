using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal.Generics
{
    public interface IGenericProcessor
    {
        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
        void SetRelationships(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
    }

    public class GenericProcessor<T> : GenericProcessor<T, int> where T : class, IIdentifiable<int>
    {
        public GenericProcessor(IDbContextResolver contextResolver) : base(contextResolver) { }
    }

    public class GenericProcessor<T, TId> : IGenericProcessor where T : class, IIdentifiable<TId>
    {
        private readonly DbContext _context;
        public GenericProcessor(IDbContextResolver contextResolver)
        {
            _context = contextResolver.GetContext();
        }

        public async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            SetRelationships(parent, relationship, relationshipIds);

            await _context.SaveChangesAsync();
        }

        public void SetRelationships(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            if (relationship.IsHasMany)
            {
                var entities = _context.GetDbSet<T>().Where(x => relationshipIds.Contains(x.StringId)).ToList();
                relationship.SetValue(parent, entities);
            }
            else
            {
                var entity = _context.GetDbSet<T>().SingleOrDefault(x => relationshipIds.First() == x.StringId);
                relationship.SetValue(parent, entity);
            }
        }
    }
}
