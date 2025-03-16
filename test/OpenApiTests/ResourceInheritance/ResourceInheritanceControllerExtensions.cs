using TestBuildingBlocks;

namespace OpenApiTests.ResourceInheritance;

public static class ResourceInheritanceControllerExtensions
{
    public static void UseInheritanceControllers(
        this IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext, bool hasOperationsController)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        testContext.UseController<DistrictsController>();
        testContext.UseController<StaffMembersController>();

        testContext.UseController<BuildingsController>();
        testContext.UseController<ResidencesController>();
        testContext.UseController<FamilyHomesController>();
        testContext.UseController<MansionsController>();

        testContext.UseController<RoomsController>();
        testContext.UseController<KitchensController>();
        testContext.UseController<BedroomsController>();
        testContext.UseController<BathroomsController>();
        testContext.UseController<LivingRoomsController>();
        testContext.UseController<ToiletsController>();

        testContext.UseController<RoadsController>();
        testContext.UseController<CyclePathsController>();

        if (hasOperationsController)
        {
            testContext.UseController<OperationsController>();
        }
    }
}
