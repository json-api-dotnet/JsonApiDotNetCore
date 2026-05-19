using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.CustomRoutes.GeneratedCode;
using OpenApiKiotaEndToEndTests.CustomRoutes.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.CustomRoutes;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.CustomRoutes;

public sealed class CustomRouteTests : IClassFixture<IntegrationTestContext<OpenApiStartup<CustomRouteDbContext>, CustomRouteDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<CustomRouteDbContext>, CustomRouteDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly CustomRouteFakers _fakers = new();

    public CustomRouteTests(IntegrationTestContext<OpenApiStartup<CustomRouteDbContext>, CustomRouteDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<ElectionsController>();
    }

    [Fact]
    public async Task Can_get_primary_resources_at_custom_route()
    {
        // Arrange
        List<Election> elections = _fakers.Election.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Election>();
            dbContext.Elections.AddRange(elections);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        ElectionCollectionResponseDocument? response = await apiClient.VotingApi.Overview.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.Should().ContainSingle(data => data.Id == elections[0].Id);
        response.Data.Should().ContainSingle(data => data.Id == elections[1].Id);
    }

    [Fact]
    public async Task Can_get_secondary_resources_at_custom_route()
    {
        // Arrange
        Election election = _fakers.Election.GenerateOne();
        election.Candidates = _fakers.Candidate.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Elections.Add(election);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        CandidateCollectionResponseDocument? response = await apiClient.VotingApi.Overview[election.Id].Contenders.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.Should().ContainSingle(data => data.Id == election.Candidates.ElementAt(0).Id);
        response.Data.Should().ContainSingle(data => data.Id == election.Candidates.ElementAt(1).Id);
    }

    [Fact]
    public async Task Can_get_election_winner()
    {
        // Arrange
        Election election = _fakers.Election.GenerateOne();
        Candidate winner = _fakers.Candidate.GenerateOne();
        Candidate loser = _fakers.Candidate.GenerateOne();

        election.Ballots = _fakers.Ballot.GenerateSet(4);
        election.Ballots.ForEach(ballot => ballot.SelectedCandidate = winner);
        election.Ballots.ElementAt(0).SelectedCandidate = loser;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Elections.Add(election);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        PrimaryCandidateResponseDocument? response = await apiClient.VotingApi.Overview.Winner[election.Id].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(winner.Id);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.PersonName.Should().Be(winner.PersonName);
        response.Data.Attributes.PartyName.Should().Be(winner.PartyName);
    }

    [Fact]
    public async Task Can_get_election_winner_when_only_one_candidate_has_votes()
    {
        // Arrange
        Election election = _fakers.Election.GenerateOne();
        Candidate candidate = _fakers.Candidate.GenerateOne();

        election.Ballots = _fakers.Ballot.GenerateSet(3);
        election.Ballots.ForEach(ballot => ballot.SelectedCandidate = candidate);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Elections.Add(election);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        PrimaryCandidateResponseDocument? response = await apiClient.VotingApi.Overview.Winner[election.Id].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(candidate.Id);
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.PersonName.Should().Be(candidate.PersonName);
        response.Data.Attributes.PartyName.Should().Be(candidate.PartyName);
    }

    [Fact]
    public async Task Cannot_get_election_winner_for_unknown_election()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.VotingApi.Overview.Winner[Unknown.TypedId.Guid].GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("404");
        error.Title.Should().Be("The election does not exist.");
        error.Detail.Should().Be($"Election with ID '{Unknown.StringId.Guid}' does not exist.");
    }

    [Fact]
    public async Task Cannot_get_election_winner_when_no_votes()
    {
        // Arrange
        Election election = _fakers.Election.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Elections.Add(election);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.VotingApi.Overview.Winner[election.Id].GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.Conflict);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("409");
        error.Title.Should().Be("No single winner found.");
        error.Detail.Should().Be("There are no votes.");
    }

    [Fact]
    public async Task Cannot_get_election_winner_when_votes_are_tied()
    {
        // Arrange
        Election election = _fakers.Election.GenerateOne();
        Candidate candidate1 = _fakers.Candidate.GenerateOne();
        Candidate candidate2 = _fakers.Candidate.GenerateOne();

        election.Ballots = _fakers.Ballot.GenerateSet(4);
        election.Ballots.ElementAt(0).SelectedCandidate = candidate1;
        election.Ballots.ElementAt(1).SelectedCandidate = candidate2;
        election.Ballots.ElementAt(2).SelectedCandidate = candidate1;
        election.Ballots.ElementAt(3).SelectedCandidate = candidate2;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Elections.Add(election);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new CustomRoutesClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.VotingApi.Overview.Winner[election.Id].GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.Conflict);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("409");
        error.Title.Should().Be("No single winner found.");
        error.Detail.Should().Be("Multiple candidates are tied for first place.");
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
