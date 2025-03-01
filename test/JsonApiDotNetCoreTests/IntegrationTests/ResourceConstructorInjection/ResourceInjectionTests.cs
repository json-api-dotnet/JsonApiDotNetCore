using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

public sealed class ResourceInjectionTests : IClassFixture<IntegrationTestContext<TestableStartup<InjectionDbContext>, InjectionDbContext>>
{
    private static readonly DateTimeOffset CurrentTime = 31.January(2021).At(17, 1).AsUtc();
    private static readonly DateTimeOffset OfficeIsOpenTime = 27.January(2021).At(13, 53).AsUtc();
    private static readonly DateTimeOffset OfficeIsClosedTime = 30.January(2021).At(21, 43).AsUtc();

    private readonly IntegrationTestContext<TestableStartup<InjectionDbContext>, InjectionDbContext> _testContext;
    private readonly InjectionFakers _fakers;

    public ResourceInjectionTests(IntegrationTestContext<TestableStartup<InjectionDbContext>, InjectionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<GiftCertificatesController>();
        testContext.UseController<PostOfficesController>();

        testContext.PostConfigureServices(services => services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new FrozenTimeProvider(CurrentTime))));

        var timeProvider = (FrozenTimeProvider)testContext.Factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.Reset();

        _fakers = new InjectionFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_get_resource_by_ID()
    {
        // Arrange
        GiftCertificate certificate = _fakers.GiftCertificate.GenerateOne();
        certificate.IssueDate = CurrentTime.AddYears(-1).AddDays(1).UtcDateTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.GiftCertificates.Add(certificate);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/giftCertificates/{certificate.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(certificate.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("issueDate").WhoseValue.Should().Be(certificate.IssueDate);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("hasExpired").WhoseValue.Should().Be(false);
    }

    [Fact]
    public async Task Can_filter_resources_by_ID()
    {
        // Arrange
        var timeProvider = (FrozenTimeProvider)_testContext.Factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.SetUtcNow(OfficeIsOpenTime);

        List<PostOffice> postOffices = _fakers.PostOffice.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<PostOffice>();
            dbContext.PostOffices.AddRange(postOffices);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/postOffices?filter=equals(id,'{postOffices[1].StringId}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(postOffices[1].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("address").WhoseValue.Should().Be(postOffices[1].Address);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("isOpen").WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task Can_get_secondary_resource_with_fieldset()
    {
        // Arrange
        var timeProvider = (FrozenTimeProvider)_testContext.Factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.SetUtcNow(OfficeIsOpenTime);

        GiftCertificate certificate = _fakers.GiftCertificate.GenerateOne();
        certificate.Issuer = _fakers.PostOffice.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.GiftCertificates.Add(certificate);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/giftCertificates/{certificate.StringId}/issuer?fields[postOffices]=id,isOpen";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(certificate.Issuer.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("isOpen").WhoseValue.Should().Be(true);
    }

    [Fact]
    public async Task Can_create_resource_with_ToOne_relationship_and_include()
    {
        // Arrange
        var timeProvider = (FrozenTimeProvider)_testContext.Factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.SetUtcNow(OfficeIsClosedTime);

        PostOffice existingOffice = _fakers.PostOffice.GenerateOne();

        DateTimeOffset newIssueDate = OfficeIsClosedTime.AddYears(-1).AddDays(-1).UtcDateTime;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PostOffices.Add(existingOffice);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "giftCertificates",
                attributes = new
                {
                    issueDate = newIssueDate
                },
                relationships = new
                {
                    issuer = new
                    {
                        data = new
                        {
                            type = "postOffices",
                            id = existingOffice.StringId
                        }
                    }
                }
            }
        };

        const string route = "/giftCertificates?include=issuer";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("issueDate").WhoseValue.Should().Be(newIssueDate);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("hasExpired").WhoseValue.Should().Be(true);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("issuer").WhoseValue.With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Id.Should().Be(existingOffice.StringId);
        });

        responseDocument.Included.Should().HaveCount(1);

        responseDocument.Included[0].With(resource =>
        {
            resource.Id.Should().Be(existingOffice.StringId);
            resource.Attributes.Should().ContainKey("address").WhoseValue.Should().Be(existingOffice.Address);
            resource.Attributes.Should().ContainKey("isOpen").WhoseValue.Should().Be(false);
        });

        int newCertificateId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            GiftCertificate certificateInDatabase = await dbContext.GiftCertificates
                .Include(certificate => certificate.Issuer).FirstWithIdAsync(newCertificateId);

            certificateInDatabase.IssueDate.Should().Be(newIssueDate);

            certificateInDatabase.Issuer.ShouldNotBeNull();
            certificateInDatabase.Issuer.Id.Should().Be(existingOffice.Id);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_ToMany_relationship()
    {
        // Arrange
        var timeProvider = (FrozenTimeProvider)_testContext.Factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.SetUtcNow(OfficeIsClosedTime);

        PostOffice existingOffice = _fakers.PostOffice.GenerateOne();
        existingOffice.GiftCertificates = _fakers.GiftCertificate.GenerateList(1);

        string newAddress = _fakers.PostOffice.GenerateOne().Address;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PostOffices.Add(existingOffice);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "postOffices",
                id = existingOffice.StringId,
                attributes = new
                {
                    address = newAddress
                },
                relationships = new
                {
                    giftCertificates = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "giftCertificates",
                                id = existingOffice.GiftCertificates[0].StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/postOffices/{existingOffice.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PostOffice officeInDatabase = await dbContext.PostOffices.Include(postOffice => postOffice.GiftCertificates).FirstWithIdAsync(existingOffice.Id);

            officeInDatabase.Address.Should().Be(newAddress);

            officeInDatabase.GiftCertificates.Should().HaveCount(1);
            officeInDatabase.GiftCertificates[0].Id.Should().Be(existingOffice.GiftCertificates[0].Id);
        });
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        PostOffice existingOffice = _fakers.PostOffice.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PostOffices.Add(existingOffice);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/postOffices/{existingOffice.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PostOffice? officeInDatabase = await dbContext.PostOffices.FirstWithIdOrDefaultAsync(existingOffice.Id);

            officeInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_delete_unknown_resource()
    {
        // Arrange
        string officeId = Unknown.StringId.For<PostOffice, int>();

        string route = $"/postOffices/{officeId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'postOffices' with ID '{officeId}' does not exist.");
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship()
    {
        // Arrange
        PostOffice existingOffice = _fakers.PostOffice.GenerateOne();
        existingOffice.GiftCertificates = _fakers.GiftCertificate.GenerateList(1);

        GiftCertificate existingCertificate = _fakers.GiftCertificate.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingOffice, existingCertificate);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "giftCertificates",
                    id = existingCertificate.StringId
                }
            }
        };

        string route = $"/postOffices/{existingOffice.StringId}/relationships/giftCertificates";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PostOffice officeInDatabase = await dbContext.PostOffices.Include(postOffice => postOffice.GiftCertificates).FirstWithIdAsync(existingOffice.Id);

            officeInDatabase.GiftCertificates.Should().HaveCount(2);
        });
    }
}
