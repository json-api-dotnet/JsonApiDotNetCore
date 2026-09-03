using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.MixedControllers.GeneratedCode;
using OpenApiKiotaEndToEndTests.MixedControllers.GeneratedCode.Models;
using OpenApiTests.MixedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using IJsonApiOptions = JsonApiDotNetCore.Configuration.IJsonApiOptions;
using JsonApiOptions = JsonApiDotNetCore.Configuration.JsonApiOptions;

namespace OpenApiKiotaEndToEndTests.MixedControllers;

public sealed class MixedControllerTests : IClassFixture<IntegrationTestContext<MixedControllerStartup, CoffeeDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<MixedControllerStartup, CoffeeDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly MixedControllerFakers _fakers = new();

    public MixedControllerTests(IntegrationTestContext<MixedControllerStartup, CoffeeDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<CupOfCoffeesController>();
        testContext.UseController<CoffeeSummaryController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownQueryStringParameters = true;
    }

    [Fact]
    public async Task Can_get_coffee_summary()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(10);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        PrimaryCoffeeSummaryResponseDocument? response = await apiClient.CoffeeSummaries.Summary.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.TotalCount.Should().Be(10);
        response.Data.Attributes.BlackCount.Should().Be(cups.Count(cup => cup is { HasMilk: false, HasSugar: false }));
        response.Data.Attributes.OnlySugarCount.Should().Be(cups.Count(cup => cup is { HasMilk: false, HasSugar: true }));
        response.Data.Attributes.OnlyMilkCount.Should().Be(cups.Count(cup => cup is { HasMilk: true, HasSugar: false }));
        response.Data.Attributes.SugarWithMilkCount.Should().Be(cups.Count(cup => cup is { HasMilk: true, HasSugar: true }));
    }

    [Fact]
    public async Task Cannot_get_empty_coffee_summary()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.CoffeeSummaries.Summary.GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("404");
        error.Title.Should().Be("No cups available to summarize.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_only_black_cups()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(2);
        cups[0].HasSugar = true;
        cups[1].HasMilk = false;
        cups[1].HasSugar = false;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        CupOfCoffeeCollectionResponseDocument? response = await apiClient.CupOfCoffees.OnlyBlack.GetAsync();

        // Assert
        response.Should().NotBeNull();

        response.Data.Should().ContainSingle().Which.With(data =>
        {
            data.Id.Should().Be(cups[1].StringId);
            data.Attributes.Should().NotBeNull();
            data.Attributes.HasMilk.Should().BeFalse();
            data.Attributes.HasSugar.Should().BeFalse();
        });
    }

    [Fact]
    public async Task Can_get_existing_black_cup()
    {
        // Arrange
        CupOfCoffee cup = _fakers.CupOfCoffee.GenerateOne();
        cup.HasSugar = false;
        cup.HasMilk = false;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.CupsOfCoffee.Add(cup);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        PrimaryCupOfCoffeeResponseDocument? response = await apiClient.CupOfCoffees.OnlyBlack[cup.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(cup.StringId);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.HasMilk.Should().BeFalse();
        response.Data.Attributes.HasSugar.Should().BeFalse();
    }

    [Fact]
    public async Task Cannot_get_unknown_black_cup()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.CupOfCoffees.OnlyBlack[Unknown.StringId.Int64].GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'cupOfCoffees' with ID '{Unknown.StringId.Int64}' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_cups_in_batch()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        CreateCupOfCoffeeRequestDocument requestBody = new()
        {
            Data = new DataInCreateCupOfCoffeeRequest
            {
                Type = ResourceType.CupOfCoffees,
                Attributes = new AttributesInCreateCupOfCoffeeRequest
                {
                    HasSugar = true,
                    HasMilk = true
                }
            }
        };

        // Act
        await apiClient.CupOfCoffees.Batch.PostAsync(requestBody, configuration => configuration.QueryParameters.Size = 3);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<CupOfCoffee> cupsInDatabase = await dbContext.CupsOfCoffee.ToListAsync();

            cupsInDatabase.Should().HaveCount(3);
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasSugar.Should().BeTrue());
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasMilk.Should().BeTrue());
        });
    }

    [Fact]
    public async Task Cannot_create_cups_with_negative_batch_size()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        CreateCupOfCoffeeRequestDocument requestBody = new()
        {
            Data = new DataInCreateCupOfCoffeeRequest
            {
                Type = ResourceType.CupOfCoffees,
                Attributes = new AttributesInCreateCupOfCoffeeRequest
                {
                    HasSugar = true,
                    HasMilk = true
                }
            }
        };

        // Act
        Func<Task> action = async () => await apiClient.CupOfCoffees.Batch.PostAsync(requestBody, configuration => configuration.QueryParameters.Size = -1);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("400");
        error.Title.Should().Be("Invalid batch size.");
        error.Detail.Should().Be("Please specify a batch size of one or higher in the query string.");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("size");
    }

    [Fact]
    public async Task Can_reset_cups_in_batch()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(5);
        cups[0].HasSugar = true;
        cups[4].HasSugar = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        await apiClient.CupOfCoffees.Batch.PatchAsync();

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<CupOfCoffee> cupsInDatabase = await dbContext.CupsOfCoffee.ToListAsync();

            cupsInDatabase.Should().HaveCount(5);
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasSugar.Should().BeFalse());
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasMilk.Should().BeFalse());
        });
    }

    [Fact]
    public async Task Can_delete_all_cups()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        await apiClient.CupOfCoffees.Batch.DeleteAsync();

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<CupOfCoffee> cupsInDatabase = await dbContext.CupsOfCoffee.ToListAsync();

            cupsInDatabase.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Cannot_delete_all_cups_when_empty()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.CupOfCoffees.Batch.DeleteAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("404");
        error.Title.Should().BeNull();
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
