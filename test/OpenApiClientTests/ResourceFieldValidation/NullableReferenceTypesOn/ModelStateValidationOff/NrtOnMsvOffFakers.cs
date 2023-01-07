using Bogus;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

internal sealed class NrtOnMsvOffFakers
{
    private readonly Lazy<Faker<ResourceAttributesInPostRequest>> _lazyPostAttributesFaker = new(() =>
        FakerFactory.Instance.Create<ResourceAttributesInPostRequest>());

    private readonly Lazy<Faker<ResourceAttributesInPatchRequest>> _lazyPatchAttributesFaker = new(() =>
        FakerFactory.Instance.Create<ResourceAttributesInPatchRequest>());

    private readonly Lazy<Faker<ToOneEmptyInRequest>> _lazyToOneFaker = new(() =>
        FakerFactory.Instance.CreateForObjectWithResourceId<ToOneEmptyInRequest, int>());

    private readonly Lazy<Faker<NullableToOneEmptyInRequest>> _lazyNullableToOneFaker = new(() =>
        FakerFactory.Instance.CreateForObjectWithResourceId<NullableToOneEmptyInRequest, int>());

    private readonly Lazy<Faker<ToManyEmptyInRequest>> _lazyToManyFaker = new(() =>
        FakerFactory.Instance.CreateForObjectWithResourceId<ToManyEmptyInRequest, int>());

    public Faker<ResourceAttributesInPostRequest> PostAttributes => _lazyPostAttributesFaker.Value;
    public Faker<ResourceAttributesInPatchRequest> PatchAttributes => _lazyPatchAttributesFaker.Value;
    public Faker<ToOneEmptyInRequest> ToOne => _lazyToOneFaker.Value;
    public Faker<NullableToOneEmptyInRequest> NullableToOne => _lazyNullableToOneFaker.Value;
    public Faker<ToManyEmptyInRequest> ToMany => _lazyToManyFaker.Value;
}
