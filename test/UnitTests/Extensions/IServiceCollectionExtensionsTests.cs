using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.Extensions
{
    public sealed class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddJsonApiInternals_Adds_All_Required_Services()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("UnitTestDb"));
            services.AddJsonApi<AppDbContext>();

            // Act
            // this is required because the DbContextResolver requires access to the current HttpContext
            // to get the request scoped DbContext instance
            services.AddScoped<IRequestScopedServiceProvider, TestScopedServiceProvider>();
            var provider = services.BuildServiceProvider();

            // Assert
            var request = provider.GetRequiredService<IJsonApiRequest>() as JsonApiRequest;
            Assert.NotNull(request);
            var resourceGraph = provider.GetService<IResourceGraph>();
            Assert.NotNull(resourceGraph);
            request.PrimaryResource = resourceGraph.GetResourceContext<TodoItem>();
            Assert.NotNull(provider.GetService<IResourceGraph>());
            Assert.NotNull(provider.GetService<IDbContextResolver>());
            Assert.NotNull(provider.GetService(typeof(IResourceRepository<TodoItem>)));
            Assert.NotNull(provider.GetService<IResourceGraph>());
            Assert.NotNull(provider.GetService<IHttpContextAccessor>());
            Assert.NotNull(provider.GetService<IMetaBuilder>());
            Assert.NotNull(provider.GetService<IJsonApiSerializerFactory>());
            Assert.NotNull(provider.GetService<IJsonApiWriter>());
            Assert.NotNull(provider.GetService<IJsonApiReader>());
            Assert.NotNull(provider.GetService<IJsonApiDeserializer>());
            Assert.NotNull(provider.GetService<IGenericServiceFactory>());
        }

        [Fact]
        public void RegisterResource_DeviatingDbContextPropertyName_RegistersCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("UnitTestDb"));
            services.AddJsonApi<AppDbContext>();

            // Act
            // this is required because the DbContextResolver requires access to the current HttpContext
            // to get the request scoped DbContext instance
            services.AddScoped<IRequestScopedServiceProvider, TestScopedServiceProvider>();
            var provider = services.BuildServiceProvider();
            var graph = provider.GetRequiredService<IResourceGraph>();
            var resourceContext = graph.GetResourceContext<Author>();

            // Assert 
            Assert.Equal("authors", resourceContext.PublicName);
        }

        [Fact]
        public void AddResourceService_Registers_All_Shorthand_Service_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceService<IntResourceService>();

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IResourceService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IResourceCommandService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IResourceQueryService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IGetAllService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IGetByIdService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IGetSecondaryService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IGetRelationshipService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(ICreateService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IUpdateService<IntResource>)));
            Assert.IsType<IntResourceService>(provider.GetRequiredService(typeof(IDeleteService<IntResource>)));
        }

        [Fact]
        public void AddResourceService_Registers_All_LongForm_Service_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceService<GuidResourceService>();

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IResourceService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IResourceCommandService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IResourceQueryService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IGetAllService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IGetByIdService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IGetSecondaryService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IGetRelationshipService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(ICreateService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IUpdateService<GuidResource, Guid>)));
            Assert.IsType<GuidResourceService>(provider.GetRequiredService(typeof(IDeleteService<GuidResource, Guid>)));
        }

        [Fact]
        public void AddResourceService_Throws_If_Type_Does_Not_Implement_Any_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act, assert
            Assert.Throws<InvalidConfigurationException>(() => services.AddResourceService<int>());
        }

        [Fact]
        public void AddResourceRepository_Registers_All_Shorthand_Repository_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceRepository<IntResourceRepository>();

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.IsType<IntResourceRepository>(provider.GetRequiredService(typeof(IResourceRepository<IntResource>)));
            Assert.IsType<IntResourceRepository>(provider.GetRequiredService(typeof(IResourceReadRepository<IntResource>)));
            Assert.IsType<IntResourceRepository>(provider.GetRequiredService(typeof(IResourceWriteRepository<IntResource>)));
        }

        [Fact]
        public void AddResourceRepository_Registers_All_LongForm_Repository_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceRepository<GuidResourceRepository>();

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.IsType<GuidResourceRepository>(provider.GetRequiredService(typeof(IResourceRepository<GuidResource, Guid>)));
            Assert.IsType<GuidResourceRepository>(provider.GetRequiredService(typeof(IResourceReadRepository<GuidResource, Guid>)));
            Assert.IsType<GuidResourceRepository>(provider.GetRequiredService(typeof(IResourceWriteRepository<GuidResource, Guid>)));
        }

        [Fact]
        public void AddJsonApi_With_Context_Uses_Resource_Type_Name_If_NoOtherSpecified()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddScoped<IRequestScopedServiceProvider, TestScopedServiceProvider>();

            // Act
            services.AddJsonApi<TestContext>();

            // Assert
            var provider = services.BuildServiceProvider();
            var resourceGraph = provider.GetRequiredService<IResourceGraph>();
            var resource = resourceGraph.GetResourceContext(typeof(IntResource));
            Assert.Equal("intResources", resource.PublicName);
        }

        public sealed class IntResource : Identifiable { }
        public class GuidResource : Identifiable<Guid> { }

        private sealed class IntResourceService : IResourceService<IntResource>
        {
            public Task<IReadOnlyCollection<IntResource>> GetAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<IntResource> GetAsync(int id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<object> GetSecondaryAsync(int id, string relationshipName, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<object> GetRelationshipAsync(int id, string relationshipName, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<IntResource> CreateAsync(IntResource resource, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task AddToToManyRelationshipAsync(int primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<IntResource> UpdateAsync(int id, IntResource resource, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task SetRelationshipAsync(int primaryId, string relationshipName, object secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task DeleteAsync(int id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task RemoveFromToManyRelationshipAsync(int primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
        }

        private sealed class GuidResourceService : IResourceService<GuidResource, Guid>
        {
            public Task<IReadOnlyCollection<GuidResource>> GetAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<GuidResource> GetAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<object> GetSecondaryAsync(Guid id, string relationshipName, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<object> GetRelationshipAsync(Guid id, string relationshipName, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<GuidResource> CreateAsync(GuidResource resource, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task AddToToManyRelationshipAsync(Guid primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<GuidResource> UpdateAsync(Guid id, GuidResource resource, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task SetRelationshipAsync(Guid primaryId, string relationshipName, object secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task DeleteAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task RemoveFromToManyRelationshipAsync(Guid primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
        }

        private sealed class IntResourceRepository : IResourceRepository<IntResource>
        {
            public Task<IReadOnlyCollection<IntResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<IntResource> GetForCreateAsync(int id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task CreateAsync(IntResource resourceFromRequest, IntResource resourceForDatabase, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<IntResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task UpdateAsync(IntResource resourceFromRequest, IntResource resourceFromDatabase, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task DeleteAsync(int id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task SetRelationshipAsync(IntResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task AddToToManyRelationshipAsync(int primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task RemoveFromToManyRelationshipAsync(IntResource primaryResource, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
        }

        private sealed class GuidResourceRepository : IResourceRepository<GuidResource, Guid>
        {
            public Task<IReadOnlyCollection<GuidResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<GuidResource> GetForCreateAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task CreateAsync(GuidResource resourceFromRequest, GuidResource resourceForDatabase, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<GuidResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task UpdateAsync(GuidResource resourceFromRequest, GuidResource resourceFromDatabase, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task DeleteAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task SetRelationshipAsync(GuidResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task AddToToManyRelationshipAsync(Guid primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task RemoveFromToManyRelationshipAsync(GuidResource primaryResource, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken) => throw new NotImplementedException();
        }

        public class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }

            public DbSet<IntResource> Resource { get; set; }
        }
    }
}
