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

        testContext.SwaggerDocumentOutputDirectory = "test/OpenApiEndToEndTests/ModelValidation";
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task String(string modelName)
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
    public async Task Non_nullable_string(string modelName)
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
    public async Task String_length_and_regex(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.userName").With(userNameEl =>
        {
            userNameEl.Should().HaveProperty("maxLength", 18);
            userNameEl.Should().HaveProperty("minLength", 3);
            userNameEl.Should().HaveProperty("pattern", "^[a-zA-Z]+$");
            userNameEl.Should().HaveProperty("type", "string");
            userNameEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Credit_card(string modelName)
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
    public async Task Email(string modelName)
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
    public async Task Phone(string modelName)
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
    public async Task Age(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.age").With(ageEl =>
        {
            ageEl.Should().HaveProperty("maximum", 123);
            ageEl.Should().HaveProperty("minimum", 0);
            ageEl.Should().HaveProperty("type", "integer");
            ageEl.Should().HaveProperty("format", "int32");
            ageEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Tags(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.tags").With(tagsEl =>
        {
            tagsEl.Should().HaveProperty("type", "array");
            tagsEl.Should().ContainPath("items").With(itemsEl =>
            {
                itemsEl.Should().HaveProperty("type", "string");
                // TODO: no length constraint?
            });
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Profile_picture(string modelName)
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
    public async Task Next_revalidation(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.nextRevalidation").With(nextRevalidationEl =>
        {
            // TODO: TimeSpan format is an akward object with all the TimeSpan public properties.
            nextRevalidationEl.Should().HaveProperty("nullable", true);
        });
    }

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Validated_at(string modelName)
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
    public async Task Validated_date_at(string modelName)
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
    public async Task Validated_time_at(string modelName)
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

    [Theory]
    [MemberData(nameof(ModelNames))]
    public async Task Signature(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.properties.signature").With(signatureEl =>
        {
            signatureEl.Should().HaveProperty("type", "string");
            // TODO: no format?
            signatureEl.Should().HaveProperty("nullable", true);
        });
    }

    public static TheoryData<string> ModelNames =>
        new()
        {
            "fingerprintAttributesInPatchRequest",
            "fingerprintAttributesInPostRequest",
            "fingerprintAttributesInResponse"
        };
}
