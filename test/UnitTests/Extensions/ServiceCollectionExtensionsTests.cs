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
            ResourceType personType = resourceGraph.GetResourceType<Person>();

            // Assert
            Assert.Equal("people", personType.PublicName);
        }

        [Fact]
        public void AddResourceService_Registers_Service_Interfaces_Of_Int32()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceService<ResourceServiceOfInt32>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IResourceService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IResourceCommandService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IResourceQueryService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IGetAllService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IGetByIdService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IGetSecondaryService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IGetRelationshipService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(ICreateService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IUpdateService<ResourceOfInt32, int>)));
            Assert.IsType<ResourceServiceOfInt32>(provider.GetRequiredService(typeof(IDeleteService<ResourceOfInt32, int>)));
        }

        [Fact]
        public void AddResourceService_Registers_Service_Interfaces_Of_Guid()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceService<ResourceServiceOfGuid>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IResourceService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IResourceCommandService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IResourceQueryService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IGetAllService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IGetByIdService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IGetSecondaryService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IGetRelationshipService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(ICreateService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IUpdateService<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceServiceOfGuid>(provider.GetRequiredService(typeof(IDeleteService<ResourceOfGuid, Guid>)));
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
        public void AddResourceRepository_Registers_Repository_Interfaces_Of_Int32()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceRepository<ResourceRepositoryOfInt32>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<ResourceRepositoryOfInt32>(provider.GetRequiredService(typeof(IResourceRepository<ResourceOfInt32, int>)));
            Assert.IsType<ResourceRepositoryOfInt32>(provider.GetRequiredService(typeof(IResourceReadRepository<ResourceOfInt32, int>)));
            Assert.IsType<ResourceRepositoryOfInt32>(provider.GetRequiredService(typeof(IResourceWriteRepository<ResourceOfInt32, int>)));
        }

        [Fact]
        public void AddResourceRepository_Registers_Repository_Interfaces_Of_Guid()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceRepository<ResourceRepositoryOfGuid>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<ResourceRepositoryOfGuid>(provider.GetRequiredService(typeof(IResourceRepository<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceRepositoryOfGuid>(provider.GetRequiredService(typeof(IResourceReadRepository<ResourceOfGuid, Guid>)));
            Assert.IsType<ResourceRepositoryOfGuid>(provider.GetRequiredService(typeof(IResourceWriteRepository<ResourceOfGuid, Guid>)));
        }

        [Fact]
        public void AddResourceDefinition_Registers_Definition_Interface_Of_Int32()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceDefinition<ResourceDefinitionOfInt32>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<ResourceDefinitionOfInt32>(provider.GetRequiredService(typeof(IResourceDefinition<ResourceOfInt32, int>)));
        }

        [Fact]
        public void AddResourceDefinition_Registers_Definition_Interface_Of_Guid()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResourceDefinition<ResourceDefinitionOfGuid>();

            // Assert
            ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsType<ResourceDefinitionOfGuid>(provider.GetRequiredService(typeof(IResourceDefinition<ResourceOfGuid, Guid>)));
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

            ResourceType intResourceType = resourceGraph.GetResourceType(typeof(ResourceOfInt32));
            Assert.Equal("resourceOfInt32s", intResourceType.PublicName);
        }

        private sealed class ResourceOfInt32 : Identifiable
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class ResourceOfGuid : Identifiable<Guid>
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class ResourceServiceOfInt32 : IResourceService<ResourceOfInt32, int>
        {
            public Task<IReadOnlyCollection<ResourceOfInt32>> GetAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfInt32> GetAsync(int id, CancellationToken cancellationToken)
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

            public Task<ResourceOfInt32> CreateAsync(ResourceOfInt32 resource, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(int leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfInt32> UpdateAsync(int id, ResourceOfInt32 resource, CancellationToken cancellationToken)
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
        private sealed class ResourceServiceOfGuid : IResourceService<ResourceOfGuid, Guid>
        {
            public Task<IReadOnlyCollection<ResourceOfGuid>> GetAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfGuid> GetAsync(Guid id, CancellationToken cancellationToken)
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

            public Task<ResourceOfGuid> CreateAsync(ResourceOfGuid resource, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(Guid leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfGuid> UpdateAsync(Guid id, ResourceOfGuid resource, CancellationToken cancellationToken)
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
        private sealed class ResourceRepositoryOfInt32 : IResourceRepository<ResourceOfInt32, int>
        {
            public Task<IReadOnlyCollection<ResourceOfInt32>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfInt32> GetForCreateAsync(int id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task CreateAsync(ResourceOfInt32 resourceFromRequest, ResourceOfInt32 resourceForDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfInt32> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task UpdateAsync(ResourceOfInt32 resourceFromRequest, ResourceOfInt32 resourceFromDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(int id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(ResourceOfInt32 leftResource, object rightValue, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(int leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(ResourceOfInt32 leftResource, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class ResourceRepositoryOfGuid : IResourceRepository<ResourceOfGuid, Guid>
        {
            public Task<IReadOnlyCollection<ResourceOfGuid>> GetAsync(QueryLayer layer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfGuid> GetForCreateAsync(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task CreateAsync(ResourceOfGuid resourceFromRequest, ResourceOfGuid resourceForDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<ResourceOfGuid> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task UpdateAsync(ResourceOfGuid resourceFromRequest, ResourceOfGuid resourceFromDatabase, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task SetRelationshipAsync(ResourceOfGuid leftResource, object rightValue, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task AddToToManyRelationshipAsync(Guid leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task RemoveFromToManyRelationshipAsync(ResourceOfGuid leftResource, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class ResourceDefinitionOfInt32 : IResourceDefinition<ResourceOfInt32, int>
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

            public QueryStringParameterHandlers<ResourceOfInt32> OnRegisterQueryableHandlersForQueryStringParameters()
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, object> GetMeta(ResourceOfInt32 resource)
            {
                throw new NotImplementedException();
            }

            public Task OnPrepareWriteAsync(ResourceOfInt32 resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IIdentifiable> OnSetToOneRelationshipAsync(ResourceOfInt32 leftResource, HasOneAttribute hasOneRelationship,
                IIdentifiable rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnSetToManyRelationshipAsync(ResourceOfInt32 leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnAddToRelationshipAsync(int leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnRemoveFromRelationshipAsync(ResourceOfInt32 leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWritingAsync(ResourceOfInt32 resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWriteSucceededAsync(ResourceOfInt32 resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void OnDeserialize(ResourceOfInt32 resource)
            {
                throw new NotImplementedException();
            }

            public void OnSerialize(ResourceOfInt32 resource)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class ResourceDefinitionOfGuid : IResourceDefinition<ResourceOfGuid, Guid>
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

            public QueryStringParameterHandlers<ResourceOfGuid> OnRegisterQueryableHandlersForQueryStringParameters()
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, object> GetMeta(ResourceOfGuid resource)
            {
                throw new NotImplementedException();
            }

            public Task OnPrepareWriteAsync(ResourceOfGuid resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IIdentifiable> OnSetToOneRelationshipAsync(ResourceOfGuid leftResource, HasOneAttribute hasOneRelationship,
                IIdentifiable rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnSetToManyRelationshipAsync(ResourceOfGuid leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnAddToRelationshipAsync(Guid leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnRemoveFromRelationshipAsync(ResourceOfGuid leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWritingAsync(ResourceOfGuid resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task OnWriteSucceededAsync(ResourceOfGuid resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void OnDeserialize(ResourceOfGuid resource)
            {
                throw new NotImplementedException();
            }

            public void OnSerialize(ResourceOfGuid resource)
            {
                throw new NotImplementedException();
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class TestDbContext : DbContext
        {
            public DbSet<ResourceOfInt32> ResourcesOfInt32 { get; set; }
            public DbSet<ResourceOfGuid> ResourcesOfGuid { get; set; }
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
