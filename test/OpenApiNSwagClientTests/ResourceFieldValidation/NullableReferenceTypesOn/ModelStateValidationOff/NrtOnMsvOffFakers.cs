using Bogus;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

internal sealed class NrtOnMsvOffFakers
{
    private readonly Lazy<Faker<AttributesInCreateResourceRequest>> _lazyPostAttributesFaker =
        new(FakerFactory.Instance.Create<AttributesInCreateResourceRequest>);

    private readonly Lazy<Faker<AttributesInUpdateResourceRequest>> _lazyPatchAttributesFaker =
        new(FakerFactory.Instance.Create<AttributesInUpdateResourceRequest>);

    private readonly Lazy<Faker<ToOneEmptyInRequest>> _lazyToOneFaker = new(FakerFactory.Instance.CreateForObjectWithResourceId<ToOneEmptyInRequest, int>);

    private readonly Lazy<Faker<NullableToOneEmptyInRequest>> _lazyNullableToOneFaker =
        new(FakerFactory.Instance.CreateForObjectWithResourceId<NullableToOneEmptyInRequest, int>);

    private readonly Lazy<Faker<ToManyEmptyInRequest>> _lazyToManyFaker = new(FakerFactory.Instance.CreateForObjectWithResourceId<ToManyEmptyInRequest, int>);

    public Faker<AttributesInCreateResourceRequest> PostAttributes => _lazyPostAttributesFaker.Value;
    public Faker<AttributesInUpdateResourceRequest> PatchAttributes => _lazyPatchAttributesFaker.Value;
    public Faker<ToOneEmptyInRequest> ToOne => _lazyToOneFaker.Value;
    public Faker<NullableToOneEmptyInRequest> NullableToOne => _lazyNullableToOneFaker.Value;
    public Faker<ToManyEmptyInRequest> ToMany => _lazyToManyFaker.Value;
}
