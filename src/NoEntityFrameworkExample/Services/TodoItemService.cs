using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Data;
using JsonApiDotNetCoreExample.Models;
using System.Linq;

namespace NoEntityFrameworkExample.Services
{
    public class TodoItemService : IResourceService<TodoItem>
    {
        private readonly string _connectionString;

        public TodoItemService(IConfiguration config)
        {
            _connectionString = config.GetValue<string>("Data:DefaultConnection");
        }

        private IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(Func<IDbConnection, Task<IEnumerable<T>>> query)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return await query(dbConnection);
            }
        }

        public async Task<IEnumerable<TodoItem>> GetAsync()
        {
            return await QueryAsync<TodoItem>(async connection =>
            {
                return await connection.QueryAsync<TodoItem>("select * from \"TodoItems\"");
            });
        }

        public async Task<TodoItem> GetAsync(int id)
        {
            return (await QueryAsync<TodoItem>(async connection =>
            {
                return await connection.QueryAsync<TodoItem>("select * from \"TodoItems\" where \"Id\"= @id", new { id });
            })).SingleOrDefault();
        }

        public Task<object> GetRelationshipAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetRelationshipsAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public Task<TodoItem> CreateAsync(TodoItem entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
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
