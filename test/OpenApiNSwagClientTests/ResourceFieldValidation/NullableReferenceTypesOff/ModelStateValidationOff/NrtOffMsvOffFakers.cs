using Bogus;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff.GeneratedCode;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff;

internal sealed class NrtOffMsvOffFakers
{
    private readonly Lazy<Faker<ResourceAttributesInPostRequest>> _lazyPostAttributesFaker = new(() =>
        FakerFactory.Instance.Create<ResourceAttributesInPostRequest>());

    private readonly Lazy<Faker<ResourceAttributesInPatchRequest>> _lazyPatchAttributesFaker = new(() =>
        FakerFactory.Instance.Create<ResourceAttributesInPatchRequest>());

    private readonly Lazy<Faker<NullableToOneEmptyInRequest>> _lazyNullableToOneFaker = new(() =>
        FakerFactory.Instance.CreateForObjectWithResourceId<NullableToOneEmptyInRequest, int>());

    private readonly Lazy<Faker<ToManyEmptyInRequest>> _lazyToManyFaker = new(() =>
        FakerFactory.Instance.CreateForObjectWithResourceId<ToManyEmptyInRequest, int>());

    public Faker<ResourceAttributesInPostRequest> PostAttributes => _lazyPostAttributesFaker.Value;
    public Faker<ResourceAttributesInPatchRequest> PatchAttributes => _lazyPatchAttributesFaker.Value;
    public Faker<NullableToOneEmptyInRequest> NullableToOne => _lazyNullableToOneFaker.Value;
    public Faker<ToManyEmptyInRequest> ToMany => _lazyToManyFaker.Value;
}
