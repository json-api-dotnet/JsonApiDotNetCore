using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
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

        public async Task<IReadOnlyCollection<WorkItem>> GetAsync(CancellationToken cancellationToken)
        {
            return (await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(new CommandDefinition(@"select * from ""WorkItems""", cancellationToken: cancellationToken)))).ToList();
        }

        public async Task<WorkItem> GetAsync(int id, CancellationToken cancellationToken)
        {
            var query = await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(new CommandDefinition(@"select * from ""WorkItems"" where ""Id""=@id", new {id}, cancellationToken: cancellationToken)));

            return query.Single();
        }

        public Task<object> GetSecondaryAsync(int id, string relationshipName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetRelationshipAsync(int id, string relationshipName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkItem> CreateAsync(WorkItem resource, CancellationToken cancellationToken)
        {
            return (await QueryAsync(async connection =>
            {
                var query =
                    @"insert into ""WorkItems"" (""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId"") values " +
                    @"(@title, @isBlocked, @durationInHours, @projectId) returning ""Id"", ""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId""";

                return await connection.QueryAsync<WorkItem>(new CommandDefinition(query, new
                {
                    title = resource.Title, isBlocked = resource.IsBlocked, durationInHours = resource.DurationInHours, projectId = resource.ProjectId
                }, cancellationToken: cancellationToken));
            })).SingleOrDefault();
        }

        public Task AddToToManyRelationshipAsync(int primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<WorkItem> UpdateAsync(int id, WorkItem resource, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRelationshipAsync(int primaryId, string relationshipName, object secondaryResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await QueryAsync(async connection =>
                await connection.QueryAsync<WorkItem>(new CommandDefinition(@"delete from ""WorkItems"" where ""Id""=@id", new {id}, cancellationToken: cancellationToken)));
        }

        public Task RemoveFromToManyRelationshipAsync(int primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
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
