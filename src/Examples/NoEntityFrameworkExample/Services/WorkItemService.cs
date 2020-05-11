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
    public sealed class WorkItemService : IResourceService<WorkItem>
    {
        private readonly string _connectionString;

        public WorkItemService(IConfiguration configuration)
        {
            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            _connectionString = configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
        }

        public async Task<IEnumerable<WorkItem>> GetAsync()
        {
            return await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(@"select * from ""WorkItems"""));
        }

        public async Task<WorkItem> GetAsync(int id)
        {
            var query = await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(@"select * from ""WorkItems"" where ""Id""=@id", new { id }));

            return query.Single();
        }

        public Task<object> GetRelationshipAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public Task<WorkItem> GetRelationshipsAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkItem> CreateAsync(WorkItem entity)
        {
            return (await QueryAsync(async connection =>
            {
                var query = @"insert into ""WorkItems"" (""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId"") values (@description, @isLocked, @ordinal, @uniqueId) returning ""Id"", ""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId""";
                var result = await connection.QueryAsync<WorkItem>(query, new { description = entity.Title, ordinal = entity.DurationInHours, uniqueId = entity.ProjectId, isLocked = entity.IsBlocked });
                return result;
            })).SingleOrDefault();
        }

        public async Task DeleteAsync(int id)
        {
            await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(@"delete from ""WorkItems"" where ""Id""=@id", new { id }));
        }

        public Task<WorkItem> UpdateAsync(int id, WorkItem entity)
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
