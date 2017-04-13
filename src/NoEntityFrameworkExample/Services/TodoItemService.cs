using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Services
{
    public class TodoItemService : IResourceService<TodoItem>
    {
        public Task<TodoItem> CreateAsync(TodoItem entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TodoItem>> GetAsync()
        {
            return Task.Run<IEnumerable<TodoItem>>(() => {
                return new List<TodoItem> {
                    new TodoItem {
                        Id = 1,
                        Description = "description"
                    }
                };
            });
        }

        public Task<TodoItem> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetRelationshipAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetRelationshipsAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public Task<TodoItem> UpdateAsync(int id, TodoItem entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRelationshipsAsync(int id, string relationshipName, List<DocumentData> relationships)
        {
            throw new NotImplementedException();
        }
    }
}
