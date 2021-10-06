using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.Extensions
{
    public sealed class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void RegisterResource_DeviatingDbContextPropertyName_RegistersCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("UnitTestDb"));

            // Act
            services.AddJsonApi<TestDbContext>();

            ServiceProvider provider = services.BuildServiceProvider();
            var resourceGraph = provider.GetRequiredService<IResourceGraph>();
            ResourceContext resourceContext = resourceGraph.GetResourceContext<Person>();

            // Assert
            Assert.Equal("people", resourceContext.PublicName);
        }

        [Fact]
        public void AddResourceService_Registers_All_Shorthand_Service_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceService<IntResourceService>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
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
            ServiceProvider provider = services.BuildServiceProvider();
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

            // Act
            Action action = () => services.AddResourceService<int>();

            // Assert
            Assert.Throws<InvalidConfigurationException>(action);
        }

        [Fact]
        public void AddResourceRepository_Registers_All_Shorthand_Repository_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceRepository<IntResourceRepository>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
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
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<GuidResourceRepository>(provider.GetRequiredService(typeof(IResourceRepository<GuidResource, Guid>)));
            Assert.IsType<GuidResourceRepository>(provider.GetRequiredService(typeof(IResourceReadRepository<GuidResource, Guid>)));
            Assert.IsType<GuidResourceRepository>(provider.GetRequiredService(typeof(IResourceWriteRepository<GuidResource, Guid>)));
        }

        [Fact]
        public void AddResourceDefinition_Registers_Shorthand_Definition_Interface()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceDefinition<IntResourceDefinition>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<IntResourceDefinition>(provider.GetRequiredService(typeof(IResourceDefinition<IntResource>)));
        }

        [Fact]
        public void AddResourceDefinition_Registers_LongForm_Definition_Interface()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceDefinition<GuidResourceDefinition>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<GuidResourceDefinition>(provider.GetRequiredService(typeof(IResourceDefinition<GuidResource, Guid>)));
        }

        [Fact]
        public void AddJsonApi_With_Context_Uses_Resource_Type_Name_If_NoOtherSpecified()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            // Act
            services.AddJsonApi<TestDbContext>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            var resourceGraph = provider.GetRequiredService<IResourceGraph>();

            ResourceContext resourceContext = resourceGraph.GetResourceContext(typeof(IntResource));
            Assert.Equal("intResources", resourceContext.PublicName);
        }

        private sealed class IntResource : Identifiable
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class GuidResource : Identifiable<Guid>
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class IntResourceService : IResourceService<IntResource>
        {
            public Task<IReadOnlyCollection<IntResource>> GetAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IntResource> GetAsync(int id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<object> GetSecondaryAsync(int id, string relationshipName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<object> GetRelationshipAsync(int id, string relationshipName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IntResource> CreateAsync(IntResource resource, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(int leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IntResource> UpdateAsync(int id, IntResource resource, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(int leftId, string relationshipName, object rightValue, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(int id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(int leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class GuidResourceService : IResourceService<GuidResource, Guid>
        {
            public Task<IReadOnlyCollection<GuidResource>> GetAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<GuidResource> GetAsync(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<object> GetSecondaryAsync(Guid id, string relationshipName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<object> GetRelationshipAsync(Guid id, string relationshipName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<GuidResource> CreateAsync(GuidResource resource, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(Guid leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<GuidResource> UpdateAsync(Guid id, GuidResource resource, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(Guid leftId, string relationshipName, object rightValue, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(Guid leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class IntResourceRepository : IResourceRepository<IntResource>
        {
            public Task<IReadOnlyCollection<IntResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IntResource> GetForCreateAsync(int id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task CreateAsync(IntResource resourceFromRequest, IntResource resourceForDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IntResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task UpdateAsync(IntResource resourceFromRequest, IntResource resourceFromDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(int id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(IntResource leftResource, object rightValue, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(int leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(IntResource leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class GuidResourceRepository : IResourceRepository<GuidResource, Guid>
        {
            public Task<IReadOnlyCollection<GuidResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<GuidResource> GetForCreateAsync(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task CreateAsync(GuidResource resourceFromRequest, GuidResource resourceForDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<GuidResource> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task UpdateAsync(GuidResource resourceFromRequest, GuidResource resourceFromDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(GuidResource leftResource, object rightValue, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(Guid leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(GuidResource leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class IntResourceDefinition : IResourceDefinition<IntResource>
        {
            public IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
            {
                throw new NotImplementedException();
            }

            public FilterExpression OnApplyFilter(FilterExpression existingFilter)
            {
                throw new NotImplementedException();
            }

            public SortExpression OnApplySort(SortExpression existingSort)
            {
                throw new NotImplementedException();
            }

            public PaginationExpression OnApplyPagination(PaginationExpression existingPagination)
            {
                throw new NotImplementedException();
            }

            public SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet)
            {
                throw new NotImplementedException();
            }

            public QueryStringParameterHandlers<IntResource> OnRegisterQueryableHandlersForQueryStringParameters()
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, object> GetMeta(IntResource resource)
            {
                throw new NotImplementedException();
            }

            public Task OnPrepareWriteAsync(IntResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IIdentifiable> OnSetToOneRelationshipAsync(IntResource leftResource, HasOneAttribute hasOneRelationship, IIdentifiable rightResourceId,
                WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnSetToManyRelationshipAsync(IntResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnAddToRelationshipAsync(int leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnRemoveFromRelationshipAsync(IntResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWritingAsync(IntResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWriteSucceededAsync(IntResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void OnDeserialize(IntResource resource)
            {
                throw new NotImplementedException();
            }

            public void OnSerialize(IntResource resource)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class GuidResourceDefinition : IResourceDefinition<GuidResource, Guid>
        {
            public IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
            {
                throw new NotImplementedException();
            }

            public FilterExpression OnApplyFilter(FilterExpression existingFilter)
            {
                throw new NotImplementedException();
            }

            public SortExpression OnApplySort(SortExpression existingSort)
            {
                throw new NotImplementedException();
            }

            public PaginationExpression OnApplyPagination(PaginationExpression existingPagination)
            {
                throw new NotImplementedException();
            }

            public SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet)
            {
                throw new NotImplementedException();
            }

            public QueryStringParameterHandlers<GuidResource> OnRegisterQueryableHandlersForQueryStringParameters()
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, object> GetMeta(GuidResource resource)
            {
                throw new NotImplementedException();
            }

            public Task OnPrepareWriteAsync(GuidResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IIdentifiable> OnSetToOneRelationshipAsync(GuidResource leftResource, HasOneAttribute hasOneRelationship, IIdentifiable rightResourceId,
                WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnSetToManyRelationshipAsync(GuidResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnAddToRelationshipAsync(Guid leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnRemoveFromRelationshipAsync(GuidResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWritingAsync(GuidResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWriteSucceededAsync(GuidResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void OnDeserialize(GuidResource resource)
            {
                throw new NotImplementedException();
            }

            public void OnSerialize(GuidResource resource)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class TestDbContext : DbContext
        {
            public DbSet<IntResource> Resource { get; set; }
            public DbSet<Person> People { get; set; }

            public TestDbContext(DbContextOptions<TestDbContext> options)
                : base(options)
            {
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        private sealed class Person : Identifiable
        {
        }
    }
}
