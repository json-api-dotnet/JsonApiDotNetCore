using Bogus;
using OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesDisabled.GeneratedCode;

namespace OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesDisabled;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

internal sealed class NullableReferenceTypesDisabledFaker
{
    private readonly Lazy<Faker<HenHousePostRequestDocument>> _lazyHenHousePostRequestDocumentFaker;
    private readonly Lazy<Faker<HenHousePatchRequestDocument>> _lazyHenHousePatchRequestDocumentFaker;

    private readonly Lazy<Faker<ChickenPostRequestDocument>> _lazyChickenPostRequestDocumentFaker = new(() =>
    {
        Faker<ChickenAttributesInPostRequest> attributesInPostRequestFaker = new Faker<ChickenAttributesInPostRequest>()
            .RuleFor(attributes => attributes.Name, faker => faker.Name.FirstName())
            .RuleFor(attributes => attributes.NameOfCurrentFarm, faker => faker.Company.CompanyName())
            .RuleFor(attributes => attributes.Age, faker => faker.Random.Int(1, 20))
            .RuleFor(attributes => attributes.Weight, faker => faker.Random.Int(20, 50))
            .RuleFor(attributes => attributes.TimeAtCurrentFarmInDays, faker => faker.Random.Int(1, 356))
            .RuleFor(attributes => attributes.HasProducedEggs, _ => true);

        Faker<ChickenDataInPostRequest> dataInPostRequestFaker = new Faker<ChickenDataInPostRequest>()
            .RuleFor(data => data.Attributes, _ => attributesInPostRequestFaker.Generate());

        return new Faker<ChickenPostRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPostRequestFaker.Generate());
    });

    private readonly Lazy<Faker<ChickenPatchRequestDocument>> _lazyChickenPatchRequestDocumentFaker = new(() =>
    {
        Faker<ChickenAttributesInPatchRequest> attributesInPatchRequestFaker = new Faker<ChickenAttributesInPatchRequest>()
            .RuleFor(attributes => attributes.Name, faker => faker.Name.FirstName())
            .RuleFor(attributes => attributes.NameOfCurrentFarm, faker => faker.Company.CompanyName())
            .RuleFor(attributes => attributes.Age, faker => faker.Random.Int(1, 20))
            .RuleFor(attributes => attributes.Weight, faker => faker.Random.Int(20, 50))
            .RuleFor(attributes => attributes.TimeAtCurrentFarmInDays, faker => faker.Random.Int(1, 356))
            .RuleFor(attributes => attributes.HasProducedEggs, _ => true);

        Faker<ChickenDataInPatchRequest> dataInPatchRequestFaker = new Faker<ChickenDataInPatchRequest>()
            // @formatter:wrap_chained_method_calls chop_if_long
            .RuleFor(data => data.Id, faker => faker.Random.Int(1, 100).ToString())
            // @formatter:wrap_chained_method_calls restore
            .RuleFor(data => data.Attributes, _ => attributesInPatchRequestFaker.Generate());

        return new Faker<ChickenPatchRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPatchRequestFaker.Generate());
    });

    private readonly Lazy<Faker<ToOneChickenInRequest>> _lazyToOneChickenInRequestFaker = new(() =>
        new Faker<ToOneChickenInRequest>()
            .RuleFor(relationship => relationship.Data, faker => new ChickenIdentifier
            {
                // @formatter:wrap_chained_method_calls chop_if_long
                Id = faker.Random.Int(1, 100).ToString()
                // @formatter:wrap_chained_method_calls restore
            }));

    private readonly Lazy<Faker<NullableToOneChickenInRequest>> _lazyNullableToOneChickenInRequestFaker = new(() =>
        new Faker<NullableToOneChickenInRequest>()
            .RuleFor(relationship => relationship.Data, faker => new ChickenIdentifier
            {
                // @formatter:wrap_chained_method_calls chop_if_long
                Id = faker.Random.Int(1, 100).ToString()
                // @formatter:wrap_chained_method_calls restore
            }));

    private readonly Lazy<Faker<ToManyChickenInRequest>> _lazyToManyChickenInRequestFaker = new(() =>
        new Faker<ToManyChickenInRequest>()
            .RuleFor(relationship => relationship.Data, faker => new List<ChickenIdentifier>
            {
                new()
                {
                    // @formatter:wrap_chained_method_calls chop_if_long
                    Id = faker.Random.Int(1, 100).ToString()
                    // @formatter:wrap_chained_method_calls restore
                }
            }));

    public Faker<ChickenPostRequestDocument> ChickenPostRequestDocument => _lazyChickenPostRequestDocumentFaker.Value;
    public Faker<ChickenPatchRequestDocument> ChickenPatchRequestDocument => _lazyChickenPatchRequestDocumentFaker.Value;
    public Faker<HenHousePostRequestDocument> HenHousePostRequestDocument => _lazyHenHousePostRequestDocumentFaker.Value;
    public Faker<HenHousePatchRequestDocument> HenHousePatchRequestDocument => _lazyHenHousePatchRequestDocumentFaker.Value;

    public NullableReferenceTypesDisabledFaker()
    {
        _lazyHenHousePostRequestDocumentFaker = new Lazy<Faker<HenHousePostRequestDocument>>(HenHousePostRequestDocumentFakerFactory);
        _lazyHenHousePatchRequestDocumentFaker = new Lazy<Faker<HenHousePatchRequestDocument>>(HenHousePatchRequestDocumentFakerFactory);
    }

    private Faker<HenHousePostRequestDocument> HenHousePostRequestDocumentFakerFactory()
    {
        Faker<HenHouseRelationshipsInPostRequest> relationshipsInPostRequestFaker = new Faker<HenHouseRelationshipsInPostRequest>()
            .RuleFor(relationships => relationships.OldestChicken, _ => _lazyNullableToOneChickenInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.FirstChicken, _ => _lazyToOneChickenInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.ChickensReadyForLaying, _ => _lazyToManyChickenInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.AllChickens, _ => _lazyToManyChickenInRequestFaker.Value.Generate());

        Faker<HenHouseDataInPostRequest> dataInPostRequestFaker = new Faker<HenHouseDataInPostRequest>()
            .RuleFor(data => data.Relationships, _ => relationshipsInPostRequestFaker.Generate());

        return new Faker<HenHousePostRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPostRequestFaker.Generate());
    }

    private Faker<HenHousePatchRequestDocument> HenHousePatchRequestDocumentFakerFactory()
    {
        Faker<HenHouseRelationshipsInPatchRequest> relationshipsInPatchRequestFaker = new Faker<HenHouseRelationshipsInPatchRequest>()
            .RuleFor(relationships => relationships.OldestChicken, _ => _lazyNullableToOneChickenInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.FirstChicken, _ => _lazyToOneChickenInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.ChickensReadyForLaying, _ => _lazyToManyChickenInRequestFaker.Value.Generate())
            .RuleFor(relationships => relationships.AllChickens, _ => _lazyToManyChickenInRequestFaker.Value.Generate());

        Faker<HenHouseDataInPatchRequest> dataInPatchRequestFaker = new Faker<HenHouseDataInPatchRequest>()
            // @formatter:wrap_chained_method_calls chop_if_long
            .RuleFor(data => data.Id, faker => faker.Random.Int(1, 100).ToString())
            // @formatter:wrap_chained_method_calls restore
            .RuleFor(data => data.Relationships, _ => relationshipsInPatchRequestFaker.Generate());

        return new Faker<HenHousePatchRequestDocument>()
            .RuleFor(document => document.Data, _ => dataInPatchRequestFaker.Generate());
    }
}
