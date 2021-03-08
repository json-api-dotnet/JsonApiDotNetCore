using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Configuration;
using NoEntityFrameworkExample.Models;
using Npgsql;

namespace NoEntityFrameworkExample.Services
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
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
            const string commandText = @"select * from ""WorkItems""";
            var commandDefinition = new CommandDefinition(commandText, cancellationToken: cancellationToken);

            return await QueryAsync(async connection => await connection.QueryAsync<WorkItem>(commandDefinition));
        }

        public async Task<WorkItem> GetAsync(int id, CancellationToken cancellationToken)
        {
            const string commandText = @"select * from ""WorkItems"" where ""Id""=@id";

            var commandDefinition = new CommandDefinition(commandText, new
            {
                id
            }, cancellationToken: cancellationToken);

            IReadOnlyCollection<WorkItem> workItems = await QueryAsync(async connection => await connection.QueryAsync<WorkItem>(commandDefinition));
            return workItems.Single();
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
            const string commandText = @"insert into ""WorkItems"" (""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId"") values " +
                @"(@title, @isBlocked, @durationInHours, @projectId) returning ""Id"", ""Title"", ""IsBlocked"", ""DurationInHours"", ""ProjectId""";

            var commandDefinition = new CommandDefinition(commandText, new
            {
                title = resource.Title,
                isBlocked = resource.IsBlocked,
                durationInHours = resource.DurationInHours,
                projectId = resource.ProjectId
            }, cancellationToken: cancellationToken);

            IReadOnlyCollection<WorkItem> workItems = await QueryAsync(async connection => await connection.QueryAsync<WorkItem>(commandDefinition));
            return workItems.Single();
        }

        public Task AddToToManyRelationshipAsync(int primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
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
            const string commandText = @"delete from ""WorkItems"" where ""Id""=@id";

            await QueryAsync(async connection => await connection.QueryAsync<WorkItem>(new CommandDefinition(commandText, new
            {
                id
            }, cancellationToken: cancellationToken)));
        }

        public Task RemoveFromToManyRelationshipAsync(int primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<IReadOnlyCollection<T>> QueryAsync<T>(Func<IDbConnection, Task<IEnumerable<T>>> query)
        {
            using IDbConnection dbConnection = new NpgsqlConnection(_connectionString);
            dbConnection.Open();

            IEnumerable<T> resources = await query(dbConnection);
            return resources.ToList();
        }
    }
}
