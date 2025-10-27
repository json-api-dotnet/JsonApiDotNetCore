using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode;
using OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ModelStateValidation;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using IJsonApiOptions = JsonApiDotNetCore.Configuration.IJsonApiOptions;

namespace OpenApiKiotaEndToEndTests.ModelStateValidation;

public sealed class ModelStateValidationTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ModelStateValidationFakers _fakers = new();

    public ModelStateValidationTests(IntegrationTestContext<OpenApiStartup<ModelStateValidationDbContext>, ModelStateValidationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<SocialMediaAccountsController>();

        var options = testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcdefghijklmnopqrstu")]
    public async Task Cannot_exceed_length_constraint(string firstName)
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    FirstName = firstName,
                    LastName = newAccount.LastName
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field FirstName must be a string or collection type with a minimum length of '2' and maximum length of '20'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/firstName");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdefghijklmnopqrs")]
    public async Task Cannot_exceed_string_length_constraint(string userName)
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    UserName = userName
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field UserName must be a string with a minimum length of 3 and a maximum length of 18.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/userName");
    }

    [Fact]
    public async Task Cannot_violate_regular_expression_constraint()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    UserName = "aB1"
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("Only letters are allowed.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/userName");
    }

    [Fact]
    public async Task Cannot_use_invalid_credit_card_number()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    CreditCard = "123-456"
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The CreditCard field is not a valid credit card number.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/creditCard");
    }

    [Fact]
    public async Task Cannot_use_invalid_email_address()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    Email = "abc"
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The Email field is not a valid e-mail address.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/email");
    }

    [Fact]
    public async Task Cannot_exceed_min_length_constraint()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    // Using -3 instead of -1 to compensate for base64 padding.
                    Password = Enumerable.Repeat((byte)'X', SocialMediaAccount.MinPasswordChars - 3).ToArray()
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        const int minCharsInBase64 = SocialMediaAccount.MinPasswordCharsInBase64;

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be($"The field Password must be a string or array type with a minimum length of '{minCharsInBase64}'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/password");
    }

    [Fact]
    public async Task Cannot_exceed_max_length_constraint()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    Password = Enumerable.Repeat((byte)'X', SocialMediaAccount.MaxPasswordChars + 1).ToArray()
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        const int maxCharsInBase64 = SocialMediaAccount.MaxPasswordCharsInBase64;

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be($"The field Password must be a string or array type with a maximum length of '{maxCharsInBase64}'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/password");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.56)]
    [InlineData(123.98)]
    [InlineData(124)]
    public async Task Cannot_use_double_outside_of_valid_range(double age)
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    Age = age
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be($"The field Age must be between {0.1} exclusive and {122.9} exclusive.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/age");
    }

    [Fact]
    public async Task Cannot_use_relative_url()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    BackgroundPicture = "relative-url"
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The BackgroundPicture field is not a valid fully-qualified http, https, or ftp URL.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/backgroundPicture");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task Cannot_exceed_collection_length_constraint(int length)
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    Tags = Enumerable.Repeat("-", length).ToList()
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field Tags must be a string or collection type with a minimum length of '1' and maximum length of '10'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/tags");
    }

    [Fact]
    public async Task Cannot_use_non_allowed_value()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    CountryCode = "XX"
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The CountryCode field does not equal any of the values specified in AllowedValuesAttribute.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/countryCode");
    }

    [Fact]
    public async Task Cannot_use_denied_value()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    Planet = "pluto"
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The Planet field equals one of the values specified in DeniedValuesAttribute.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/planet");
    }

    [Fact]
    public async Task Cannot_use_TimeSpan_outside_of_valid_range()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    LastName = newAccount.LastName,
                    NextRevalidation = TimeSpan.FromSeconds(1).ToString()
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("422");
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field NextRevalidation must be between 01:00:00 and 05:00:00.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/nextRevalidation");
    }

    [Fact]
    public async Task Can_create_resource_with_valid_properties()
    {
        // Arrange
        SocialMediaAccount newAccount = _fakers.SocialMediaAccount.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        ModelStateValidationClient apiClient = new(requestAdapter);

        var requestBody = new CreateSocialMediaAccountRequestDocument
        {
            Data = new DataInCreateSocialMediaAccountRequest
            {
                Type = SocialMediaAccountResourceType.SocialMediaAccounts,
                Attributes = new AttributesInCreateSocialMediaAccountRequest
                {
                    AlternativeId = newAccount.AlternativeId,
                    FirstName = newAccount.FirstName,
                    LastName = newAccount.LastName,
                    UserName = newAccount.UserName,
                    CreditCard = newAccount.CreditCard,
                    Email = newAccount.Email,
                    Password = Convert.FromBase64String(newAccount.Password!),
                    Phone = newAccount.Phone,
                    Age = newAccount.Age,
                    ProfilePicture = newAccount.ProfilePicture!.ToString(),
                    BackgroundPicture = newAccount.BackgroundPicture,
                    Tags = newAccount.Tags,
                    CountryCode = newAccount.CountryCode,
                    Planet = newAccount.Planet,
                    NextRevalidation = newAccount.NextRevalidation!.Value.ToString(),
                    ValidatedAt = newAccount.ValidatedAt!,
                    ValidatedAtDate = newAccount.ValidatedAtDate!.Value,
                    ValidatedAtTime = newAccount.ValidatedAtTime!.Value
                }
            }
        };

        // Act
        Func<Task> action = () => apiClient.SocialMediaAccounts.PostAsync(requestBody);

        // Assert
        await action.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
