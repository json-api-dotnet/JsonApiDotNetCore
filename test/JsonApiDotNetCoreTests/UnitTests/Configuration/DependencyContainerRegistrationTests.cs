using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Configuration;

public sealed class DependencyContainerRegistrationTests
{
    [Fact]
    public void Can_resolve_registered_services_from_example_project()
    {
        // Arrange
        IHostBuilder hostBuilder = CreateValidatingHostBuilder();

        // Act
        using IHost _ = hostBuilder.Build();
    }

    [Fact]
    public void Cannot_resolve_registered_services_with_conflicting_scopes()
    {
        // Arrange
        IHostBuilder hostBuilder = CreateValidatingHostBuilder();

        hostBuilder.ConfigureServices(services =>
        {
            services.AddScoped<SomeScopedService>();
            services.AddSingleton<SomeSingletonService>();
        });

        // Act
        Action action = () =>
        {
            using IHost _ = hostBuilder.Build();
        };

        // Assert
        action.Should().ThrowExactly<AggregateException>()
            .WithMessage("Some services are not able to be constructed * Singleton * Cannot consume scoped service *");
    }

    [Fact]
    public void Cannot_resolve_registered_services_with_circular_dependency()
    {
        // Arrange
        IHostBuilder hostBuilder = CreateValidatingHostBuilder();

        hostBuilder.ConfigureServices(services =>
        {
            services.AddScoped<CircularServiceA>();
            services.AddScoped<CircularServiceB>();
        });

        // Act
        Action action = () =>
        {
            using IHost _ = hostBuilder.Build();
        };

        // Assert
        action.Should().ThrowExactly<AggregateException>().WithMessage("Some services are not able to be constructed * A circular dependency was detected *");
    }

    [Fact]
    public void Can_replace_enumerable_service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddScoped<IFilterQueryStringParameterReader, CustomFilterQueryStringParameterReader>();

        // Act
        services.AddJsonApi();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        IQueryStringParameterReader[] parameterReaders = provider.GetRequiredService<IEnumerable<IQueryStringParameterReader>>().ToArray();
        parameterReaders.Should().NotContain(parameterReader => parameterReader is FilterQueryStringParameterReader);
        parameterReaders.Should().ContainSingle(parameterReader => parameterReader is CustomFilterQueryStringParameterReader);

        IQueryConstraintProvider[] constraintProviders = provider.GetRequiredService<IEnumerable<IQueryConstraintProvider>>().ToArray();
        constraintProviders.Should().NotContain(constraintProvider => constraintProvider is FilterQueryStringParameterReader);
        constraintProviders.Should().ContainSingle(constraintProvider => constraintProvider is CustomFilterQueryStringParameterReader);
    }

    [Fact]
    public void Can_add_enumerable_service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddScoped<IQueryStringParameterReader, CustomFilterQueryStringParameterReader>();
        services.AddScoped<IQueryConstraintProvider, CustomFilterQueryStringParameterReader>();

        // Act
        services.AddJsonApi();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        IQueryStringParameterReader[] parameterReaders = provider.GetRequiredService<IEnumerable<IQueryStringParameterReader>>().ToArray();
        parameterReaders.Should().ContainSingle(parameterReader => parameterReader is CustomFilterQueryStringParameterReader);
        parameterReaders.Should().ContainSingle(parameterReader => parameterReader is FilterQueryStringParameterReader);

        IQueryConstraintProvider[] constraintProviders = provider.GetRequiredService<IEnumerable<IQueryConstraintProvider>>().ToArray();
        constraintProviders.Should().ContainSingle(constraintProvider => constraintProvider is CustomFilterQueryStringParameterReader);
        constraintProviders.Should().ContainSingle(constraintProvider => constraintProvider is FilterQueryStringParameterReader);
    }

    private static IHostBuilder CreateValidatingHostBuilder()
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureServices(services =>
                services.AddDbContext<DependencyContainerRegistrationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString())));

            webBuilder.UseStartup<TestableStartup<DependencyContainerRegistrationDbContext>>();

            webBuilder.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
        });

        return hostBuilder;
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class SomeSingletonService
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        public SomeSingletonService(SomeScopedService scopedService)
        {
            ArgumentNullException.ThrowIfNull(scopedService);
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class SomeScopedService;

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class CircularServiceA
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        public CircularServiceA(CircularServiceB serviceB)
        {
            ArgumentNullException.ThrowIfNull(serviceB);
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class CircularServiceB
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        public CircularServiceB(CircularServiceA serviceA)
        {
            ArgumentNullException.ThrowIfNull(serviceA);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DependencyContainerRegistrationDbContext(DbContextOptions<DependencyContainerRegistrationDbContext> options)
        : TestableDbContext(options)
    {
        public DbSet<Resource> Resources => Set<Resource>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Resource : Identifiable<int>
    {
        [Attr]
        public string? Field { get; set; }
    }

    [UsedImplicitly(ImplicitUseKindFlags.Access)]
    private sealed class CustomFilterQueryStringParameterReader : IFilterQueryStringParameterReader
    {
        public bool AllowEmptyValue => throw new NotImplementedException();

        public bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            throw new NotImplementedException();
        }

        public bool CanRead(string parameterName)
        {
            throw new NotImplementedException();
        }

        public void Read(string parameterName, StringValues parameterValue)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            throw new NotImplementedException();
        }
    }
}
