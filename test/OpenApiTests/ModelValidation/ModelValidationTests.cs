using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ModelValidation;

public sealed class ModelValidationTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ModelValidationDbContext>, ModelValidationDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ModelValidationDbContext>, ModelValidationDbContext> _testContext;

    public ModelValidationTests(OpenApiTestContext<OpenApiStartup<ModelValidationDbContext>, ModelValidationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FingerprintsController>();

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Nullable_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.firstName").With(firstNameEl =>
        {
            firstNameEl.Should().HaveProperty("type", "string");
            firstNameEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Required_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.lastName").With(lastNameEl =>
        {
            lastNameEl.Should().HaveProperty("minLength", 1);
            lastNameEl.Should().HaveProperty("type", "string");
            lastNameEl.Should().NotContainPath("nullable");
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task StringLength_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.userName").With(userNameEl =>
        {
            userNameEl.Should().HaveProperty("maxLength", 18);
            userNameEl.Should().HaveProperty("minLength", 3);
            userNameEl.Should().HaveProperty("type", "string");
            userNameEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task RegularExpression_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.userName").With(userNameEl =>
        {
            userNameEl.Should().HaveProperty("pattern", "^[a-zA-Z]+$");
            userNameEl.Should().HaveProperty("type", "string");
            userNameEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task CreditCard_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.creditCard").With(creditCardEl =>
        {
            creditCardEl.Should().HaveProperty("type", "string");
            creditCardEl.Should().HaveProperty("format", "credit-card");
            creditCardEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Email_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.email").With(emailEl =>
        {
            emailEl.Should().HaveProperty("type", "string");
            emailEl.Should().HaveProperty("format", "email");
            emailEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Phone_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.phone").With(phoneEl =>
        {
            phoneEl.Should().HaveProperty("type", "string");
            phoneEl.Should().HaveProperty("format", "tel");
            phoneEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Range_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.age").With(ageEl =>
        {
            ageEl.Should().HaveProperty("maximum", 123);
            ageEl.Should().NotContainPath("exclusiveMaximum");
            ageEl.Should().HaveProperty("minimum", 0);
            ageEl.Should().NotContainPath("exclusiveMinimum");
            ageEl.Should().HaveProperty("type", "integer");
            ageEl.Should().HaveProperty("format", "int32");
            ageEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Url_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.profilePicture").With(profilePictureEl =>
        {
            profilePictureEl.Should().HaveProperty("type", "string");
            profilePictureEl.Should().HaveProperty("format", "uri");
            profilePictureEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Uri_type_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.backgroundPicture").With(backgroundPictureEl =>
        {
            backgroundPictureEl.Should().HaveProperty("type", "string");
            backgroundPictureEl.Should().HaveProperty("format", "uri");
            backgroundPictureEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task TimeSpan_range_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.nextRevalidation").With(nextRevalidationEl =>
        {
            nextRevalidationEl.Should().HaveProperty("type", "string");
            nextRevalidationEl.Should().HaveProperty("format", "date-span");
            nextRevalidationEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task DateTime_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.validatedAt").With(validatedAtEl =>
        {
            validatedAtEl.Should().HaveProperty("type", "string");
            validatedAtEl.Should().HaveProperty("format", "date-time");
            validatedAtEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task DateOnly_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.validatedDateAt").With(validatedDateAtEl =>
        {
            validatedDateAtEl.Should().HaveProperty("type", "string");
            validatedDateAtEl.Should().HaveProperty("format", "date");
            validatedDateAtEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task TimeOnly_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.validatedTimeAt").With(validatedTimeAtEl =>
        {
            validatedTimeAtEl.Should().HaveProperty("type", "string");
            validatedTimeAtEl.Should().HaveProperty("format", "time");
            validatedTimeAtEl.Should().HaveProperty("nullable", true);
        });
    }

    public static TheoryData<string> ModelNames =>
        new()
        {
            "fingerprintAttributesInPostRequest",
            "fingerprintAttributesInPatchRequest",
            "fingerprintAttributesInResponse"
        };
}
