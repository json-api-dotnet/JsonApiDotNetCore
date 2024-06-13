using System.Globalization;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using OpenApiNSwagEndToEndTests.ModelStateValidation.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ModelStateValidation;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ModelStateValidation;

public sealed class ModelStateValidationTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ModelStateValidationFakers _fakers = new();

    public ModelStateValidationTests(IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<SocialMediaAccountsController>();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcdefghijklmnopqrstu")]
    public async Task Cannot_exceed_length_constraint(string firstName)
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    FirstName = firstName,
                    LastName = newAccount.LastName
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The field FirstName must be a string or collection type with a minimum length of '2' and maximum length of '20'.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/firstName");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdefghijklmnopqrs")]
    public async Task Cannot_exceed_string_length_constraint(string userName)
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    UserName = userName
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The field UserName must be a string with a minimum length of 3 and a maximum length of 18.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/userName");
    }

    [Fact]
    public async Task Cannot_violate_regular_expression_constraint()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    UserName = "aB1"
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("Only letters are allowed.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/userName");
    }

    [Fact]
    public async Task Cannot_use_invalid_credit_card_number()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    CreditCard = "123-456"
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The CreditCard field is not a valid credit card number.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/creditCard");
    }

    [Fact]
    public async Task Cannot_use_invalid_email_address()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    Email = "abc"
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The Email field is not a valid e-mail address.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/email");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.56)]
    [InlineData(123.98)]
    [InlineData(124)]
    public async Task Cannot_use_double_outside_of_valid_range(int age)
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    Age = age
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The field Age must be between 0.1 and 122.9.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/age");
    }

    [Fact]
    public async Task Cannot_use_relative_url()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    BackgroundPicture = new Uri("/justapath", UriKind.Relative)
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The BackgroundPicture field is not a valid fully-qualified http, https, or ftp URL.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/backgroundPicture");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task Cannot_exceed_collection_length_constraint(int length)
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    Tags = Enumerable.Repeat("-", length).ToArray()
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The field Tags must be a string or collection type with a minimum length of '1' and maximum length of '10'.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/tags");
    }

    [Fact]
    public async Task Cannot_use_non_allowed_value()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    CountryCode = "XX"
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The CountryCode field does not equal any of the values specified in AllowedValuesAttribute.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/countryCode");
    }

    [Fact]
    public async Task Cannot_use_denied_value()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    Planet = "pluto"
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The Planet field equals one of the values specified in DeniedValuesAttribute.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/planet");
    }

    [Fact]
    public async Task Cannot_use_TimeSpan_outside_of_valid_range()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    NextRevalidation = TimeSpan.FromSeconds(1).ToString()
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The field NextRevalidation must be between 01:00:00 and 05:00:00.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/nextRevalidation");
    }

    [Fact]
    public async Task Cannot_use_culture_sensitive_TimeSpan()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    NextRevalidation = new TimeSpan(0, 2, 0, 0, 1).ToString("g", new CultureInfo("fr-FR"))
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");

        errorObject.Detail.Should()
            .Be("Failed to convert attribute 'nextRevalidation' with value '2:00:00,001' of type 'String' to type 'Nullable<TimeSpan>'.");

        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/nextRevalidation");
    }

    [Fact]
    public async Task Cannot_use_invalid_TimeOnly()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = newAccount.LastName,
                    ValidatedAtTime = TimeSpan.FromSeconds(-1)
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        errorObject.Detail.Should().Be("Failed to convert attribute 'validatedAtTime' with value '-00:00:01' of type 'String' to type 'Nullable<TimeOnly>'.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/validatedAtTime");
    }

    [Fact]
    public async Task Can_create_resource_with_valid_properties()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.Generate();

        SocialMediaAccountPostRequestDocument requestBody = new()
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    AlternativeId = newAccount.AlternativeId,
                    FirstName = newAccount.FirstName,
                    LastName = newAccount.LastName,
                    UserName = newAccount.UserName,
                    CreditCard = newAccount.CreditCard,
                    Email = newAccount.Email,
                    Password = newAccount.Password,
                    Phone = newAccount.Phone,
                    Age = newAccount.Age,
                    ProfilePicture = newAccount.ProfilePicture,
                    BackgroundPicture = new Uri(newAccount.BackgroundPicture!),
                    Tags = newAccount.Tags,
                    CountryCode = newAccount.CountryCode,
                    Planet = newAccount.Planet,
                    NextRevalidation = newAccount.NextRevalidation!.Value.ToString(),
                    ValidatedAt = newAccount.ValidatedAt!,
                    ValidatedAtDate = new DateTimeOffset(newAccount.ValidatedAtDate!.Value.ToDateTime(new TimeOnly()), TimeSpan.Zero),
                    ValidatedAtTime = newAccount.ValidatedAtTime!.Value.ToTimeSpan()
                }
            }
        };

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(requestBody);

        // Assert
        await action.Should().NotThrowAsync();
    }
}
