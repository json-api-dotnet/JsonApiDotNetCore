using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Services
{
    public class MyModelService : IResourceService<MyModel>
    {
        public Task<MyModel> CreateAsync(MyModel entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MyModel>> GetAsync()
        {
            return Task.Run<IEnumerable<MyModel>>(() => {
                return new List<MyModel> {
                    new MyModel {
                        Id = 1,
                        Description = "description"
                    }
                };
            });
        }

        public Task<MyModel> GetAsync(int id)
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

        public Task<MyModel> UpdateAsync(int id, MyModel entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRelationshipsAsync(int id, string relationshipName, List<DocumentData> relationships)
        {
            throw new NotImplementedException();
        }
    }
}
