using FluentAssertions;
using JsonApiDotNetCore.Errors;
using Xunit;

namespace OpenApiTests.OpenApiGenerationFailures.MissingFromBody;

public sealed class MissingFromBodyOnPostMethodTests : OpenApiTestContext<OpenApiStartup<MissingFromBodyDbContext>, MissingFromBodyDbContext>
{
    public MissingFromBodyOnPostMethodTests()
    {
        UseController<MissingFromBodyOnPostController>();
    }

    [Fact]
    public async Task Cannot_use_Post_controller_action_method_without_FromBody_attribute()
    {
        // Act
        Func<Task> action = async () => _ = await GetSwaggerDocumentAsync();

        // Assert
        string? actionMethod = typeof(MissingFromBodyOnPostController).GetMethod(nameof(MissingFromBodyOnPostController.AlternatePostAsync))!.ToString();
        string containingType = typeof(MissingFromBodyOnPostController).ToString();

        await action.Should().ThrowExactlyAsync<InvalidConfigurationException>().WithMessage(
            $"The action method '{actionMethod}' on type '{containingType}' contains no parameter with a [FromBody] attribute.");
    }
}
