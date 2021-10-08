using System;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Configuration
{
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
            action.Should().ThrowExactly<AggregateException>().WithMessage("Some services are not able to be constructed * " +
                "Singleton * Cannot consume scoped service *");
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
            action.Should().ThrowExactly<AggregateException>().WithMessage("Some services are not able to be constructed * " +
                "A circular dependency was detected *");
        }

        private static IHostBuilder CreateValidatingHostBuilder()
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                    services.AddDbContext<DependencyContainerRegistrationDbContext>(options => options.UseInMemoryDatabase("db")));

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
                ArgumentGuard.NotNull(scopedService, nameof(scopedService));
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class SomeScopedService
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class CircularServiceA
        {
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            public CircularServiceA(CircularServiceB serviceB)
            {
                ArgumentGuard.NotNull(serviceB, nameof(serviceB));
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class CircularServiceB
        {
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            public CircularServiceB(CircularServiceA serviceA)
            {
                ArgumentGuard.NotNull(serviceA, nameof(serviceA));
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class DependencyContainerRegistrationDbContext : DbContext
        {
            public DbSet<Resource> Resources { get; set; }

            public DependencyContainerRegistrationDbContext(DbContextOptions<DependencyContainerRegistrationDbContext> options)
                : base(options)
            {
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class Resource : Identifiable<int>
        {
            [Attr]
            public string Field { get; set; }
        }
    }
}
