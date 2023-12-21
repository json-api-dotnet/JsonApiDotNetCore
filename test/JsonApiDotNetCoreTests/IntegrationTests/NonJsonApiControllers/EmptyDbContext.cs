using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class EmptyDbContext(DbContextOptions<EmptyDbContext> options) : TestableDbContext(options);
