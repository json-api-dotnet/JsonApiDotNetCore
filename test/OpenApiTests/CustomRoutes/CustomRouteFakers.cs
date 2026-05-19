using Bogus;
using Bogus.Extensions.UnitedStates;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.CustomRoutes;

public sealed class CustomRouteFakers
{
    private readonly Lazy<Faker<Election>> _lazyElectionFaker = new(() => new Faker<Election>()
        .MakeDeterministic()
        .RuleFor(election => election.Date, faker => faker.Date.FutureDateOnly()));

    private readonly Lazy<Faker<Candidate>> _lazyCandidateFaker = new(() => new Faker<Candidate>()
        .MakeDeterministic()
        .RuleFor(candidate => candidate.PersonName, faker => faker.Person.FullName)
        .RuleFor(candidate => candidate.PartyName, faker => faker.Commerce.Department()));

    private readonly Lazy<Faker<Ballot>> _lazyBallotFaker = new(() => new Faker<Ballot>()
        .MakeDeterministic()
        .RuleFor(ballot => ballot.VoterSocialSecurityNumber, faker => faker.Person.Ssn()));

    public Faker<Election> Election => _lazyElectionFaker.Value;
    public Faker<Candidate> Candidate => _lazyCandidateFaker.Value;
    public Faker<Ballot> Ballot => _lazyBallotFaker.Value;
}
