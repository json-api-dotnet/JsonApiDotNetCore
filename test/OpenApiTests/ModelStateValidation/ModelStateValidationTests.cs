using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ModelStateValidation;

public sealed class ModelStateValidationTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> _testContext;

    public ModelStateValidationTests(OpenApiTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SocialMediaAccountsController>();

        const string targetFramework =
#if NET8_0_OR_GREATER
            "net8.0";
#else
            "net6.0";
#endif
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger/{targetFramework}";
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Guid_type_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.alternativeId").With(alternativeIdEl =>
        {
            alternativeIdEl.Should().HaveProperty("type", "string");
            alternativeIdEl.Should().HaveProperty("format", "uuid");
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Compare_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.givenName").With(surnameEl =>
        {
            surnameEl.Should().HaveProperty("type", "string");
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Required_annotation_with_AllowEmptyStrings_disabled_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.lastName").With(lastNameEl =>
        {
            lastNameEl.Should().HaveProperty("minLength", 1);
            lastNameEl.Should().HaveProperty("type", "string");
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
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Base64String_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.password").With(passwordEl =>
        {
            passwordEl.Should().HaveProperty("type", "string");
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
            ageEl.Should().HaveProperty("maximum", 122.9);
            ageEl.Should().NotContainPath("exclusiveMaximum");
            ageEl.Should().HaveProperty("minimum", 0.1);
            ageEl.Should().NotContainPath("exclusiveMinimum");
            ageEl.Should().HaveProperty("type", "number");
            ageEl.Should().HaveProperty("format", "double");
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
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task HashSet_annotated_with_Length_AllowedValues_DeniedValues_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.tags").With(tagsEl =>
        {
            tagsEl.Should().HaveProperty("uniqueItems", true);
            tagsEl.Should().HaveProperty("type", "array");
            tagsEl.Should().ContainPath("items").With(itemsEl =>
            {
                itemsEl.Should().HaveProperty("type", "string");
            });
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
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task DateOnly_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.validatedAtDate").With(validatedDateAtEl =>
        {
            validatedDateAtEl.Should().HaveProperty("type", "string");
            validatedDateAtEl.Should().HaveProperty("format", "date");
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task TimeOnly_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.validatedAtTime").With(validatedTimeAtEl =>
        {
            validatedTimeAtEl.Should().HaveProperty("type", "string");
            validatedTimeAtEl.Should().HaveProperty("format", "time");
        });
    }

    public static TheoryData<string> ModelNames =>
        // ReSharper disable once UseCollectionExpression
        new()
        {
            "socialMediaAccountAttributesInPostRequest",
            "socialMediaAccountAttributesInPatchRequest",
            "socialMediaAccountAttributesInResponse"
        };
}
