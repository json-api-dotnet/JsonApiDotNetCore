using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

public sealed class ModelAnnotationTests : IClassFixture<IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext> _testContext;
    private readonly ModelStateFakers _fakers = new();

    public ModelAnnotationTests(IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SocialMediaAccountsController>();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcdefghijklmnopqrstu")]
    public async Task Cannot_exceed_length_constraint(string testFirstName)
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    firstName = testFirstName,
                    lastName = newLastName
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field FirstName must be a string or collection type with a minimum length of '2' and maximum length of '20'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/firstName");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdefghijklmnopqrs")]
    public async Task Cannot_exceed_string_length_constraint(string testUserName)
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    userName = testUserName
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field UserName must be a string with a minimum length of 3 and a maximum length of 18.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/userName");
    }

    [Fact]
    public async Task Cannot_violate_regular_expression_constraint()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        const string newUserName = "aB1";

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    userName = newUserName
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("Only letters are allowed.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/userName");
    }

    [Fact]
    public async Task Cannot_use_invalid_credit_card_number()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        const string newCreditCard = "123-456";

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    creditCard = newCreditCard
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The CreditCard field is not a valid credit card number.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/creditCard");
    }

    [Fact]
    public async Task Cannot_use_invalid_email_address()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        const string newEmail = "abc";

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    email = newEmail
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The Email field is not a valid e-mail address.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/email");
    }

    [Fact]
    public async Task Cannot_exceed_min_length_constraint()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;

        // Using -3 instead of -1 to compensate for base64 padding.
        string newPassword = Convert.ToBase64String(Enumerable.Repeat((byte)'X', SocialMediaAccount.MinPasswordChars - 3).ToArray());

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    password = newPassword
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        const int minCharsInBase64 = SocialMediaAccount.MinPasswordCharsInBase64;

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be($"The field Password must be a string or array type with a minimum length of '{minCharsInBase64}'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/password");
    }

    [Fact]
    public async Task Cannot_exceed_max_length_constraint()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        string newPassword = Convert.ToBase64String(Enumerable.Repeat((byte)'X', SocialMediaAccount.MaxPasswordChars + 1).ToArray());

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    password = newPassword
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        const int maxCharsInBase64 = SocialMediaAccount.MaxPasswordCharsInBase64;

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
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
    public async Task Cannot_use_double_outside_of_valid_range(double testAge)
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    age = testAge
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be($"The field Age must be between {0.1} exclusive and {122.9} exclusive.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/age");
    }

    [Fact]
    public async Task Cannot_use_relative_url()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        const string newBackgroundPicture = "relativeurl";

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    backgroundPicture = newBackgroundPicture
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The BackgroundPicture field is not a valid fully-qualified http, https, or ftp URL.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/backgroundPicture");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task Cannot_exceed_collection_length_constraint(int testLength)
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        string[] newTags = Enumerable.Repeat("-", testLength).ToArray();

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    tags = newTags
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field Tags must be a string or collection type with a minimum length of '1' and maximum length of '10'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/tags");
    }

    [Fact]
    public async Task Cannot_use_non_allowed_value()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        const string newCountryCode = "XX";

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    countryCode = newCountryCode
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The CountryCode field does not equal any of the values specified in AllowedValuesAttribute.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/countryCode");
    }

    [Fact]
    public async Task Cannot_use_denied_value()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        const string newPlanet = "pluto";

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    planet = newPlanet
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The Planet field equals one of the values specified in DeniedValuesAttribute.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/planet");
    }

    [Fact]
    public async Task Cannot_use_TimeSpan_outside_of_valid_range()
    {
        // Arrange
        string newLastName = _fakers.SocialMediaAccount.GenerateOne().LastName;
        TimeSpan newNextRevalidation = TimeSpan.FromSeconds(1);

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    lastName = newLastName,
                    nextRevalidation = newNextRevalidation
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
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

        var requestBody = new
        {
            data = new
            {
                type = "socialMediaAccounts",
                attributes = new
                {
                    alternativeId = newAccount.AlternativeId,
                    firstName = newAccount.FirstName,
                    lastName = newAccount.LastName,
                    userName = newAccount.UserName,
                    creditCard = newAccount.CreditCard,
                    email = newAccount.Email,
                    password = Convert.FromBase64String(newAccount.Password!),
                    phone = newAccount.Phone,
                    age = newAccount.Age,
                    profilePicture = newAccount.ProfilePicture,
                    backgroundPicture = newAccount.BackgroundPicture,
                    tags = newAccount.Tags,
                    countryCode = newAccount.CountryCode,
                    planet = newAccount.Planet,
                    nextRevalidation = newAccount.NextRevalidation,
                    validatedAt = newAccount.ValidatedAt,
                    validatedAtDate = newAccount.ValidatedAtDate,
                    validatedAtTime = newAccount.ValidatedAtTime
                }
            }
        };

        const string route = "/socialMediaAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("alternativeId").WhoseValue.Should().Be(newAccount.AlternativeId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("firstName").WhoseValue.Should().Be(newAccount.FirstName);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("lastName").WhoseValue.Should().Be(newAccount.LastName);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("userName").WhoseValue.Should().Be(newAccount.UserName);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("creditCard").WhoseValue.Should().Be(newAccount.CreditCard);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("email").WhoseValue.Should().Be(newAccount.Email);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("password").WhoseValue.Should().Be(newAccount.Password);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("phone").WhoseValue.Should().Be(newAccount.Phone);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("age").WhoseValue.Should().Be(newAccount.Age);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("profilePicture").WhoseValue.Should().Be(newAccount.ProfilePicture);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("backgroundPicture").WhoseValue.Should().Be(newAccount.BackgroundPicture);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("tags").WhoseValue.Should().BeEquivalentTo(newAccount.Tags);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("countryCode").WhoseValue.Should().Be(newAccount.CountryCode);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("planet").WhoseValue.Should().Be(newAccount.Planet);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("nextRevalidation").WhoseValue.Should().Be(newAccount.NextRevalidation);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("validatedAt").WhoseValue.Should().Be(newAccount.ValidatedAt);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("validatedAtDate").WhoseValue.Should().Be(newAccount.ValidatedAtDate);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("validatedAtTime").WhoseValue.Should().Be(newAccount.ValidatedAtTime);

        Guid newAccountId = Guid.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            SocialMediaAccount accountInDatabase = await dbContext.SocialMediaAccounts.FirstWithIdAsync(newAccountId);

            accountInDatabase.AlternativeId.Should().Be(newAccount.AlternativeId);
            accountInDatabase.FirstName.Should().Be(newAccount.FirstName);
            accountInDatabase.LastName.Should().Be(newAccount.LastName);
            accountInDatabase.UserName.Should().Be(newAccount.UserName);
            accountInDatabase.CreditCard.Should().Be(newAccount.CreditCard);
            accountInDatabase.Email.Should().Be(newAccount.Email);
            accountInDatabase.Password.Should().Be(newAccount.Password);
            accountInDatabase.Phone.Should().Be(newAccount.Phone);
            accountInDatabase.Age.Should().Be(newAccount.Age);
            accountInDatabase.ProfilePicture.Should().Be(newAccount.ProfilePicture);
            accountInDatabase.BackgroundPicture.Should().Be(newAccount.BackgroundPicture);
            accountInDatabase.Tags.Should().BeEquivalentTo(newAccount.Tags);
            accountInDatabase.CountryCode.Should().Be(newAccount.CountryCode);
            accountInDatabase.Planet.Should().Be(newAccount.Planet);
            accountInDatabase.NextRevalidation.Should().Be(newAccount.NextRevalidation);
            accountInDatabase.ValidatedAt.Should().Be(newAccount.ValidatedAt);
            accountInDatabase.ValidatedAtDate.Should().Be(newAccount.ValidatedAtDate);
            accountInDatabase.ValidatedAtTime.Should().Be(newAccount.ValidatedAtTime);
        });
    }
}
