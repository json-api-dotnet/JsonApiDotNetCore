using Bogus;
using OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesEnabled.GeneratedCode;

namespace OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesEnabled;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

internal sealed class NullableReferenceTypesEnabledFaker
{
    private readonly Lazy<Faker<CowStablePostRequestDocument>> _lazyCowStablePostRequestDocumentFaker;
    private readonly Lazy<Faker<CowStablePatchRequestDocument>> _lazyCowStablePatchRequestDocumentFaker;

    private readonly Lazy<Faker<CowPostRequestDocument>> _lazyCowPostRequestDocumentFaker = new(() =>
    {
        Faker<CowAttributesInPostRequest> attributesInPostRequestFaker = new Faker<CowAttributesInPostRequest>()
            .RuleFor(attributes => attributes.Name, faker => faker.Name.FirstName())
            .RuleFor(attributes => attributes.NameOfCurrentFarm, faker => faker.Company.CompanyName())
            .RuleFor(attributes => attributes.NameOfPreviousFarm, faker => faker.Company.CompanyName())
            .RuleFor(attributes => attributes.Nickname, faker => faker.Internet.UserName())
            .RuleFor(attributes => attributes.Age, faker => faker.Random.Int(1, 20))
            .RuleFor(attributes => attributes.Weight, faker => faker.Random.Int(20, 50))
            .RuleFor(attributes => attributes.TimeAtCurrentFarmInDays, faker => faker.Random.Int(1, 356))
            .RuleFor(attributes => attributes.HasProducedMilk, _ => true);

        Faker<CowDataInPostRequest> dataInPostRequestFaker = new Faker<CowDataInPostRequest>()
            .RuleFor(data => data.Attributes, _ => attributesInPostRequestFaker.Generate());

        return new Faker<CowPostRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPostRequestFaker.Generate());
    });

    private readonly Lazy<Faker<CowPatchRequestDocument>> _lazyCowPatchRequestDocumentFaker = new(() =>
    {
        Faker<CowAttributesInPatchRequest> attributesInPatchRequestFaker = new Faker<CowAttributesInPatchRequest>()
            .RuleFor(attributes => attributes.Name, faker => faker.Name.FirstName())
            .RuleFor(attributes => attributes.NameOfCurrentFarm, faker => faker.Company.CompanyName())
            .RuleFor(attributes => attributes.NameOfPreviousFarm, faker => faker.Company.CompanyName())
            .RuleFor(attributes => attributes.Nickname, faker => faker.Internet.UserName())
            .RuleFor(attributes => attributes.Age, faker => faker.Random.Int(1, 20))
            .RuleFor(attributes => attributes.Weight, faker => faker.Random.Int(20, 50))
            .RuleFor(attributes => attributes.TimeAtCurrentFarmInDays, faker => faker.Random.Int(1, 356))
            .RuleFor(attributes => attributes.HasProducedMilk, _ => true);

        Faker<CowDataInPatchRequest> dataInPatchRequestFaker = new Faker<CowDataInPatchRequest>()
            // @formatter:wrap_chained_method_calls chop_if_long
            .RuleFor(data => data.Id, faker => faker.Random.Int(1, 100).ToString())
            // @formatter:wrap_chained_method_calls restore
            .RuleFor(data => data.Attributes, _ => attributesInPatchRequestFaker.Generate());

        return new Faker<CowPatchRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPatchRequestFaker.Generate());
    });

    private readonly Lazy<Faker<ToOneCowInRequest>> _lazyToOneCowInRequestFaker = new(() =>
        new Faker<ToOneCowInRequest>()
            .RuleFor(relationship => relationship.Data, faker => new CowIdentifier
            {
                // @formatter:wrap_chained_method_calls chop_if_long
                Id = faker.Random.Int(1, 100).ToString()
                // @formatter:wrap_chained_method_calls restore
            }));

    private readonly Lazy<Faker<NullableToOneCowInRequest>> _lazyNullableToOneCowInRequestFaker = new(() =>
        new Faker<NullableToOneCowInRequest>()
            .RuleFor(relationship => relationship.Data, faker => new CowIdentifier
            {
                // @formatter:wrap_chained_method_calls chop_if_long
                Id = faker.Random.Int(1, 100).ToString()
                // @formatter:wrap_chained_method_calls restore
            }));

    private readonly Lazy<Faker<ToManyCowInRequest>> _lazyToManyCowInRequestFaker = new(() =>
        new Faker<ToManyCowInRequest>()
            .RuleFor(relationship => relationship.Data, faker => new List<CowIdentifier>
            {
                new()
                {
                    // @formatter:wrap_chained_method_calls chop_if_long
                    Id = faker.Random.Int(1, 100).ToString()
                    // @formatter:wrap_chained_method_calls restore
                }
            }));

    public Faker<CowPostRequestDocument> CowPostRequestDocument => _lazyCowPostRequestDocumentFaker.Value;
    public Faker<CowPatchRequestDocument> CowPatchRequestDocument => _lazyCowPatchRequestDocumentFaker.Value;
    public Faker<CowStablePostRequestDocument> CowStablePostRequestDocument => _lazyCowStablePostRequestDocumentFaker.Value;
    public Faker<CowStablePatchRequestDocument> CowStablePatchRequestDocument => _lazyCowStablePatchRequestDocumentFaker.Value;

    public NullableReferenceTypesEnabledFaker()
    {
        _lazyCowStablePostRequestDocumentFaker = new Lazy<Faker<CowStablePostRequestDocument>>(CreateCowStablePostRequestDocumentFaker);
        _lazyCowStablePatchRequestDocumentFaker = new Lazy<Faker<CowStablePatchRequestDocument>>(CreateCowStablePatchRequestDocumentFaker);
    }

    private Faker<CowStablePostRequestDocument> CreateCowStablePostRequestDocumentFaker()
    {
        Faker<CowStableRelationshipsInPostRequest> relationshipsInPostRequestFaker = new Faker<CowStableRelationshipsInPostRequest>()
            .RuleFor(relationships => relationships.OldestCow, _ => _lazyToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.FirstCow, _ => _lazyToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.AlbinoCow, _ => _lazyNullableToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.FavoriteCow, _ => _lazyToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.CowsReadyForMilking, _ => _lazyToManyCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.AllCows, _ => _lazyToManyCowInRequestFaker.Value.Generate());

        Faker<CowStableDataInPostRequest> dataInPostRequestFaker = new Faker<CowStableDataInPostRequest>()
            .RuleFor(data => data.Relationships, _ => relationshipsInPostRequestFaker.Generate());

        return new Faker<CowStablePostRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPostRequestFaker.Generate());
    }

    private Faker<CowStablePatchRequestDocument> CreateCowStablePatchRequestDocumentFaker()
    {
        Faker<CowStableRelationshipsInPatchRequest> relationshipsInPatchRequestFaker = new Faker<CowStableRelationshipsInPatchRequest>()
            .RuleFor(relationships => relationships.OldestCow, _ => _lazyToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.FirstCow, _ => _lazyToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.AlbinoCow, _ => _lazyNullableToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.FavoriteCow, _ => _lazyToOneCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.CowsReadyForMilking, _ => _lazyToManyCowInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.AllCows, _ => _lazyToManyCowInRequestFaker.Value.Generate());

        Faker<CowStableDataInPatchRequest> dataInPatchRequestFaker = new Faker<CowStableDataInPatchRequest>()
            // @formatter:wrap_chained_method_calls chop_if_long
            .RuleFor(data => data.Id, faker => faker.Random.Int(1, 100).ToString())
            // @formatter:wrap_chained_method_calls restore
            .RuleFor(data => data.Relationships, _ => relationshipsInPatchRequestFaker.Generate());

        return new Faker<CowStablePatchRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPatchRequestFaker.Generate());
    }
}
