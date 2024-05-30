using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagEndToEndTests.ModelStateValidation.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ModelStateValidation;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ModelStateValidation;

public sealed class ModelStateValidationTests : IClassFixture<IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ModelStateValidationFakers _fakers = new();

    public ModelStateValidationTests(IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<SocialMediaAccountsController>();
    }

    [Fact]
    public async Task Cannot_violate_compare_constraint()
    {
        // Arrange
        SocialMediaAccount socialMediaAccount = _fakers.SocialMediaAccount.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    FirstName = socialMediaAccount.FirstName,
                    GivenName = "something else",
                    LastName = ""
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("'GivenName' and 'FirstName' do not match.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/givenName");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcdefghijklmnopqrstu")]
    public async Task Cannot_exceed_length_constraint(string firstName)
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    FirstName = firstName,
                    GivenName = firstName,
                    LastName = ""
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    UserName = userName,
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    UserName = "aB1",
                }
            }
        });

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
    public async Task Cannot_use_invalid_credit_card()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    CreditCard = "123-456",
                }
            }
        });

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
    public async Task Cannot_use_invalid_email()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    Email = "abc",
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    Age = age,
                }
            }
        });

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
    public async Task Cannot_use_invalid_url()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    BackgroundPicture = new Uri("/justapath", UriKind.Relative),
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    Tags = Enumerable.Repeat("", length).ToArray(),
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    CountryCode = "XX"
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    Planet = "pluto"
                }
            }
        });

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

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    NextRevalidation = "00:00:01",
                }
            }
        });

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
    public async Task Cannot_use_invalid_TimeOnly()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    LastName = "",
                    ValidatedAtTime = TimeSpan.FromSeconds(-1),
                }
            }
        });

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
        SocialMediaAccount socialMediaAccount = _fakers.SocialMediaAccount.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        ModelStateValidationClient apiClient = new(httpClient);

        // Act
        Func<Task<SocialMediaAccountPrimaryResponseDocument>> action = () => apiClient.PostSocialMediaAccountAsync(new SocialMediaAccountPostRequestDocument
        {
            Data = new SocialMediaAccountDataInPostRequest
            {
                Attributes = new SocialMediaAccountAttributesInPostRequest
                {
                    FirstName = socialMediaAccount.FirstName,
                    GivenName = socialMediaAccount.GivenName,
                    LastName = socialMediaAccount.LastName,
                    UserName = socialMediaAccount.UserName,
                    CreditCard = socialMediaAccount.CreditCard,
                    Email = socialMediaAccount.Email,
                    Phone = socialMediaAccount.Phone,
                    Age = socialMediaAccount.Age,
                    ProfilePicture = socialMediaAccount.ProfilePicture,
                    BackgroundPicture = new Uri(socialMediaAccount.BackgroundPicture!),
                    Tags = socialMediaAccount.Tags,
                    Planet = socialMediaAccount.Planet,
                    NextRevalidation = "02:00:00",
                    ValidatedAt = socialMediaAccount.ValidatedAt!,
                    ValidatedAtDate = new DateTimeOffset(socialMediaAccount.ValidatedAtDate!.Value.ToDateTime(new TimeOnly()).ToUniversalTime()),
                    ValidatedAtTime = socialMediaAccount.ValidatedAtTime!.Value.ToTimeSpan()
                }
            }
        });

        // Assert
        await action.Should().NotThrowAsync();
    }
}
