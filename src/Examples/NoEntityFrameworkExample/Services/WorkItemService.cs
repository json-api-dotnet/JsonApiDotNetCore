using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using JsonApiDotNetCore.Resources;
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

        public async Task<IReadOnlyCollection<WorkItem>> GetAsync()
        {
            return (await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(@"select * from ""WorkItems"""))).ToList();
        }

        public async Task<WorkItem> GetAsync(int id)
        {
            var query = await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(@"select * from ""WorkItems"" where ""Id""=@id", new { id }));

            return query.Single();
        }

        public Task<object> GetSecondaryAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public Task<WorkItem> GetRelationshipAsync(int id, string relationshipName)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkItem> CreateAsync(WorkItem resource)
        {
            return (await QueryAsync(async connection =>
            {
                var query = @"insert into ""WorkItems"" (""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId"") values (@description, @isLocked, @ordinal, @uniqueId) returning ""Id"", ""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId""";
                var result = await connection.QueryAsync<WorkItem>(query, new { description = resource.Title, ordinal = resource.DurationInHours, uniqueId = resource.ProjectId, isLocked = resource.IsBlocked });
                return result;
            })).SingleOrDefault();
        }

        public async Task DeleteAsync(int id)
        {
            await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(@"delete from ""WorkItems"" where ""Id""=@id", new { id }));
        }

        public Task<WorkItem> UpdateAsync(int id, WorkItem requestResource)
        {
            throw new NotImplementedException();
        }

        public Task SetRelationshipAsync(int id, string relationshipName, object newValue)
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
        public Task AddRelationshipAsync(int id, string relationshipName, IEnumerable<IIdentifiable> relationshipAssignment)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRelationshipAsync(int id, string relationshipName, IEnumerable<IIdentifiable> removals)
        {
            throw new NotImplementedException();
        }

        public Task AddRelationshipAsync(int id, string relationshipName, IReadOnlyCollection<IIdentifiable> newValues)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRelationshipAsync(int id, string relationshipName, IReadOnlyCollection<IIdentifiable> removalValues)
        {
            throw new NotImplementedException();
        }
    }
}
