using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace TestBuildingBlocks;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class EmptyDbContext(DbContextOptions<EmptyDbContext> options)
    : TestableDbContext(options);
