using FluentAssertions;
using JsonApiDotNetCore.Errors;
using Xunit;

namespace OpenApiTests.OpenApiGenerationFailures.MissingFromBody;

public sealed class MissingFromBodyOnPatchMethodTests : OpenApiTestContext<OpenApiStartup<MissingFromBodyDbContext>, MissingFromBodyDbContext>
{
    public MissingFromBodyOnPatchMethodTests()
    {
        UseController<MissingFromBodyOnPatchController>();
    }

    [Fact]
    public async Task Cannot_use_Patch_controller_action_method_without_FromBody_attribute()
    {
        // Act
        Func<Task> action = async () => _ = await GetSwaggerDocumentAsync();

        // Assert
        string? actionMethod = typeof(MissingFromBodyOnPatchController).GetMethod(nameof(MissingFromBodyOnPatchController.AlternatePatchAsync))!.ToString();
        string containingType = typeof(MissingFromBodyOnPatchController).ToString();

        await action.Should().ThrowExactlyAsync<InvalidConfigurationException>().WithMessage(
            $"The action method '{actionMethod}' on type '{containingType}' contains no parameter with a [FromBody] attribute.");
    }
}
