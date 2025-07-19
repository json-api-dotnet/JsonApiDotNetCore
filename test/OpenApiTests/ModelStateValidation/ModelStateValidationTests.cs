using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.ModelStateValidation;

public sealed class ModelStateValidationTests : IClassFixture<OpenApiTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext>>
{
    // ReSharper disable once UseCollectionExpression (https://youtrack.jetbrains.com/issue/RSRP-497450)
    public static readonly TheoryData<string> SchemaNames = new()
    {
        "attributesInCreateSocialMediaAccountRequest",
        "attributesInUpdateSocialMediaAccountRequest",
        "attributesInSocialMediaAccountResponse"
    };

    private readonly OpenApiTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> _testContext;

    public ModelStateValidationTests(OpenApiTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<SocialMediaAccountsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Guid_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.alternativeId").With(alternativeIdElement =>
        {
            alternativeIdElement.Should().HaveProperty("type", "string");
            alternativeIdElement.Should().HaveProperty("format", "uuid");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Length_annotation_on_resource_string_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.firstName").With(firstNameElement =>
        {
            firstNameElement.Should().HaveProperty("type", "string");
            firstNameElement.Should().HaveProperty("maxLength", 20);
            firstNameElement.Should().HaveProperty("minLength", 2);
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Required_annotation_with_AllowEmptyStrings_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.lastName").With(lastNameElement =>
        {
            lastNameElement.Should().HaveProperty("type", "string");
            lastNameElement.Should().NotContainPath("minLength");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task StringLength_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.userName").With(userNameElement =>
        {
            userNameElement.Should().HaveProperty("type", "string");
            userNameElement.Should().HaveProperty("maxLength", 18);
            userNameElement.Should().HaveProperty("minLength", 3);
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task RegularExpression_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.userName").With(userNameElement =>
        {
            userNameElement.Should().HaveProperty("type", "string");
            userNameElement.Should().HaveProperty("pattern", "^[a-zA-Z]+$");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task CreditCard_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.creditCard").With(creditCardElement =>
        {
            creditCardElement.Should().HaveProperty("type", "string");
            creditCardElement.Should().HaveProperty("format", "credit-card");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Email_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.email").With(emailElement =>
        {
            emailElement.Should().HaveProperty("type", "string");
            emailElement.Should().HaveProperty("format", "email");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Min_max_length_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.password").With(passwordElement =>
        {
            passwordElement.Should().HaveProperty("type", "string");
            passwordElement.Should().HaveProperty("format", "byte");
            passwordElement.Should().HaveProperty("maxLength", SocialMediaAccount.MaxPasswordCharsInBase64);
            passwordElement.Should().HaveProperty("minLength", SocialMediaAccount.MinPasswordCharsInBase64);
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Phone_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.phone").With(phoneElement =>
        {
            phoneElement.Should().HaveProperty("type", "string");
            phoneElement.Should().HaveProperty("format", "tel");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Range_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.age").With(ageElement =>
        {
            ageElement.Should().HaveProperty("type", "number");
            ageElement.Should().HaveProperty("format", "double");
            ageElement.Should().HaveProperty("maximum", 122.9);
            ageElement.Should().HaveProperty("minimum", 0.1);
            ageElement.Should().ContainPath("exclusiveMaximum").With(exclusiveElement => exclusiveElement.Should().Be(true));
            ageElement.Should().ContainPath("exclusiveMinimum").With(exclusiveElement => exclusiveElement.Should().Be(true));
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Url_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.profilePicture").With(profilePictureElement =>
        {
            profilePictureElement.Should().HaveProperty("type", "string");
            profilePictureElement.Should().HaveProperty("format", "uri");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Uri_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.backgroundPicture").With(backgroundPictureElement =>
        {
            backgroundPictureElement.Should().HaveProperty("type", "string");
            backgroundPictureElement.Should().HaveProperty("format", "uri");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Length_annotation_on_resource_list_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.tags").With(tagsElement =>
        {
            tagsElement.Should().HaveProperty("type", "array");
            tagsElement.Should().HaveProperty("maxItems", 10);
            tagsElement.Should().HaveProperty("minItems", 1);

            tagsElement.Should().ContainPath("items").With(itemsElement =>
            {
                itemsElement.Should().HaveProperty("type", "string");
            });
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task TimeSpan_range_annotation_on_resource_property_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.nextRevalidation").With(nextRevalidationElement =>
        {
            nextRevalidationElement.Should().HaveProperty("type", "string");
            nextRevalidationElement.Should().HaveProperty("format", "date-span");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task DateTime_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.validatedAt").With(validatedAtElement =>
        {
            validatedAtElement.Should().HaveProperty("type", "string");
            validatedAtElement.Should().HaveProperty("format", "date-time");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task DateOnly_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.validatedAtDate").With(validatedDateAtElement =>
        {
            validatedDateAtElement.Should().HaveProperty("type", "string");
            validatedDateAtElement.Should().HaveProperty("format", "date");
        });
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task TimeOnly_type_produces_expected_schema(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties.validatedAtTime").With(validatedTimeAtElement =>
        {
            validatedTimeAtElement.Should().HaveProperty("type", "string");
            validatedTimeAtElement.Should().HaveProperty("format", "time");
        });
    }
}
