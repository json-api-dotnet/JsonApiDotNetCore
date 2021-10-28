using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenApiClientTests.NamingConvention.KebabCase.GeneratedCode;
using OpenApiTests.NamingConvention;
using OpenApiTests.NamingConvention.KebabCase;
using TestBuildingBlocks;
using Xunit;
using GeneratedSupermarketType = OpenApiClientTests.NamingConvention.KebabCase.GeneratedCode.SupermarketType;

namespace OpenApiClientTests.NamingConvention.KebabCase
{
    public sealed class KebabCaseRoundTripTests
        : IClassFixture<IntegrationTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
    {
        private const string HostPrefix = "http://localhost/";
        private readonly IntegrationTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;
        private readonly NamingConventionFakers _fakers = new();

        public KebabCaseRoundTripTests(
            IntegrationTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
        {
            _testContext = testContext;
            testContext.UseController<SupermarketsController>();
        }

        [Fact]
        public async Task Can_use_get_collection_endpoint_with_kebab_case_naming_convention()
        {
            // Arrange
            Supermarket supermarket = _fakers.Supermarket.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Supermarket>();
                dbContext.Supermarkets.Add(supermarket);
                await dbContext.SaveChangesAsync();
            });

            KebabCaseClient apiClient = new(_testContext.Factory.CreateClient());

            // Act
            SupermarketCollectionResponseDocument resourceCollection = await apiClient.GetSupermarketCollectionAsync();

            // Assert
            resourceCollection.Links.First.Should().Be($"{HostPrefix}supermarkets");
            resourceCollection.Links.Self.Should().Be($"{HostPrefix}supermarkets");
            resourceCollection.Data.Count.Should().Be(1);

            SupermarketDataInResponse resourceDataInResponse = resourceCollection.Data.First();

            resourceDataInResponse.Links.Self.Should().Be($"{HostPrefix}supermarkets/{supermarket.StringId}");

            resourceDataInResponse.Attributes.Kind.Should().Be(Enum.Parse<GeneratedSupermarketType>(supermarket.Kind.ToString()));
            resourceDataInResponse.Attributes.NameOfCity.Should().Be(supermarket.NameOfCity);

            resourceDataInResponse.Relationships.Cashiers.Links.Self.Should().Be($"{HostPrefix}supermarkets/{supermarket.StringId}/relationships/cashiers");
            resourceDataInResponse.Relationships.Cashiers.Links.Related.Should().Be($"{HostPrefix}supermarkets/{supermarket.StringId}/cashiers");

            resourceDataInResponse.Relationships.StoreManager.Links.Self.Should()
                .Be($"{HostPrefix}supermarkets/{supermarket.StringId}/relationships/store-manager");

            resourceDataInResponse.Relationships.StoreManager.Links.Related.Should().Be($"{HostPrefix}supermarkets/{supermarket.StringId}/store-manager");
        }
    }
}
