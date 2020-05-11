using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Configuration;
using NoEntityFrameworkExample.Models;
using Npgsql;

namespace NoEntityFrameworkExample.Services
{
    public sealed class TodoItemService : IResourceService<TodoItem>
    {
        private readonly string _connectionString;

        public TodoItemService(IConfiguration configuration)
        {
            _connectionString = configuration["Data:DefaultConnection"];
        }

        public async Task<IEnumerable<TodoItem>> GetAsync()
        {
            return await QueryAsync(async connection =>
                await connection.QueryAsync<TodoItem>(@"select * from ""TodoItems"""));
        }

        public async Task<TodoItem> GetAsync(int id)
        {
            var query = await QueryAsync(async connection =>
                await connection.QueryAsync<TodoItem>(@"select * from ""TodoItems"" where ""Id""=@id", new { id }));

            return query.Single();
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
            return (await QueryAsync(async connection =>
            {
                var query = @"insert into ""TodoItems"" (""Description"", ""IsLocked"", ""Ordinal"", ""UniqueId"") values (@description, @isLocked, @ordinal, @uniqueId) returning ""Id"", ""Description"", ""IsLocked"", ""Ordinal"", ""UniqueId""";
                var result = await connection.QueryAsync<TodoItem>(query, new { description = entity.Description, ordinal = entity.Ordinal, uniqueId = entity.UniqueId, isLocked = entity.IsLocked });
                return result;
            })).SingleOrDefault();
        }

        public async Task DeleteAsync(int id)
        {
            await QueryAsync(async connection =>
                await connection.QueryAsync<TodoItem>(@"delete from ""TodoItems"" where ""Id""=@id", new { id }));
        }

        public Task<TodoItem> UpdateAsync(int id, TodoItem entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRelationshipsAsync(int id, string relationshipName, object relationships)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(Func<IDbConnection, Task<IEnumerable<T>>> query)
        {
            using IDbConnection dbConnection = GetConnection;
            dbConnection.Open();
            return await query(dbConnection);
        }

        private IDbConnection GetConnection => new NpgsqlConnection(_connectionString);
    }
}
