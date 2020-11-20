using System;
using System.Collections.Generic;
using System.Linq;
using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GettingStarted
{
    public sealed class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SampleDbContext>(
                options =>
                {
                    options.EnableSensitiveDataLogging();
                    options.UseNpgsql(
                        "Host=localhost;Port=5432;Database=JsonApiDotNetCoreExample;User ID=postgres;Password=postgres",
                        innerOptions => innerOptions.SetPostgresVersion(new Version(9, 6)));
                });

            services.AddJsonApi<SampleDbContext>(
                options =>
                {
                    options.Namespace = "api/v1";
                    options.UseRelativeLinks = true;
                    options.IncludeTotalResourceCount = true;
                    options.SerializerSettings.Formatting = Formatting.Indented;

                    // Bug workaround for https://github.com/dotnet/efcore/issues/21026
                    options.DefaultPageSize = null;

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, SampleDbContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            CreateSampleData(context);

            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static void CreateSampleData(SampleDbContext context)
        {
            // Note: The generate-examples.ps1 script (to create example requests in documentation) depends on these.

            context.Books.AddRange(new Book
            {
                Title = "Frankenstein",
                PublishYear = 1818,
                Author = new Person
                {
                    Name = "Mary Shelley"
                }
            }, new Book
            {
                Title = "Robinson Crusoe",
                PublishYear = 1719,
                Author = new Person
                {
                    Name = "Daniel Defoe"
                }
            }, new Book
            {
                Title = "Gulliver's Travels",
                PublishYear = 1726,
                Author = new Person
                {
                    Name = "Jonathan Swift"
                }
            });

            if (!context.UserProfiles.Any())
            {
                context.UserProfiles.AddRange(new UserProfile
                {
                    SubjectId = "S1",
                    FirstName = "F1",
                    LastName = "L1",
                    Email = "@1",
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            Role = new Role
                            {
                                Name = "R1"
                            }
                        },
                        new UserRole
                        {
                            Role = new Role
                            {
                                Name = "R2"
                            }
                        },
                    }
                });
            }

            context.SaveChanges();
        }
    }
}
