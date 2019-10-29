using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Data;
using NoEntityFrameworkExample.Models;
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

        public Task<TodoItem> GetRelationshipsAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public async Task<TodoItem> CreateAsync(TodoItem entity)
        {
            return (await QueryAsync<TodoItem>(async connection =>
            {
                var query = "insert into \"TodoItems\" (\"Description\", \"IsLocked\", \"Ordinal\", \"GuidProperty\") values (@description, @isLocked, @ordinal, @guidProperty) returning \"Id\",\"Description\", \"IsLocked\", \"Ordinal\", \"GuidProperty\"";
                var result = await connection.QueryAsync<TodoItem>(query, new { description = entity.Description, ordinal = entity.Ordinal, guidProperty =  entity.GuidProperty, isLocked = entity.IsLocked});
                return result;
            })).SingleOrDefault();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<TodoItem> UpdateAsync(int id, TodoItem entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRelationshipsAsync(int id, string relationshipName, object relationships)
        {
            throw new NotImplementedException();
        }
    }
}
