using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Configuration;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.ModelStateValidation
{
    public sealed class AtomicModelStateValidationStartup : TestableStartup<OperationsDbContext>
    {
        public AtomicModelStateValidationStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.ValidateModelState = true;
        }
    }
}
