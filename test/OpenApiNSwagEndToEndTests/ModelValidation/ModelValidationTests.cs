using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagEndToEndTests.ModelValidation.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ModelValidation;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiEndToEndTests.ModelValidation;

public sealed class ModelValidationTests : IClassFixture<IntegrationTestContext<OpenApiStartup<ModelValidationDbContext>, ModelValidationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ModelValidationDbContext>, ModelValidationDbContext> _testContext;
    private readonly ModelValidationFakers _fakers = new();

    public ModelValidationTests(IntegrationTestContext<OpenApiStartup<ModelValidationDbContext>, ModelValidationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FingerprintsController>();
    }

    [Fact]
    public async Task Omitting_a_required_attribute_should_return_an_error()
    {
        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                }
            }
        });

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        assertion.Which.Message.Should().Be("Cannot write a null value for property 'lastName'. Property requires a value. Path 'data.attributes'.");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdefghijklmnopqrs")]
    public async Task imbadathis(string userName)
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    UserName = userName
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
    public async Task imbadathis2()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    UserName = "aB1"
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
    public async Task imbadathis3()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    CreditCard = "123-456"
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
    public async Task imbadathis5()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    Email = "abc"
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
    [InlineData(124)]
    public async Task imbadathis6(int age)
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    Age = age
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The field Age must be between 0 and 123.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/age");
    }

    [Fact]
    public async Task imbadathis7()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    ProfilePicture = new Uri("/justapath", UriKind.Relative)
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("The ProfilePicture field is not a valid fully-qualified http, https, or ftp URL.");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/profilePicture");
    }

    [Fact]
    public async Task imbadathis8()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    NextRevalidation = new OpenApiNSwagEndToEndTests.ModelValidation.GeneratedCode.TimeSpan { TotalSeconds = 1 }
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/nextRevalidation");
    }

    [Fact]
    public async Task imbadathis10()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    ValidatedAt = DateTimeOffset.MinValue
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/");
    }

    [Fact]
    public async Task imbadathis11()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    ValidatedDateAt = DateTimeOffset.Now
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/");
    }

    [Fact]
    public async Task imbadathis9()
    {
        // Arrange
        Fingerprint fingerprint = _fakers.Fingerprint.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new ModelValidationClient(httpClient);

        // Act
        Func<Task<FingerprintPrimaryResponseDocument>> action = () => apiClient.PostFingerprintAsync(null, new FingerprintPostRequestDocument
        {
            Data = new FingerprintDataInPostRequest
            {
                Attributes = new FingerprintAttributesInPostRequest
                {
                    LastName = fingerprint.LastName,
                    ValidatedTimeAt = System.TimeSpan.FromSeconds(-1)
                }
            }
        });

        // Assert
        ErrorResponseDocument document = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which.Result;
        document.Errors.ShouldHaveCount(1);

        ErrorObject errorObject = document.Errors.First();
        errorObject.Title.Should().Be("Input validation failed.");
        errorObject.Detail.Should().Be("");
        errorObject.Source.ShouldNotBeNull();
        errorObject.Source.Pointer.Should().Be("/data/attributes/");
    }
}
