using System.Diagnostics;
using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

#pragma warning disable format

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SampleDbContext>(options =>
{
    options.UseSqlite("Data Source=SampleDb.db;Pooling=False");
    SetDbContextDebugOptions(options);
});

builder.Services.AddJsonApi<SampleDbContext>(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;

#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
    options.SerializerOptions.WriteIndented = true;
#endif
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateDatabaseAsync(app.Services);

await app.RunAsync();

[Conditional("DEBUG")]
static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.ConfigureWarnings(builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
}

static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    await CreateSampleDataAsync(dbContext);
}

static async Task CreateSampleDataAsync(SampleDbContext dbContext)
{
    // Note: The generate-examples.ps1 script (to create example requests in documentation) depends on these.

    dbContext.Books.AddRange(new Book
        {
            Title = "Frankenstein",
            PublishYear = 1818,
            Author = new Person
            {
                Name = "Mary Shelley",
                LivingAddress = new Address
                {
                    Street = "SomeStreet",
                    PostalCode = "1234 AB",
                    Country = new Country
                    {
                        Code = "NLD",
                        DisplayName = "The Netherlands",
                        Provinces =
                        [
                            new Province
                            {
                                Name = "Zuid-Holland"
                            },
                            new Province
                            {
                                Name = "Noord-Holland"
                            }
                        ]
                    },
                    NotExposed = "NotExposed"
                },
                MailAddress = new Address
                {
                    Street = "MailStreet",
                    PostalCode = "MailPostalCode",
                    Country = new Country
                    {
                        Code = "MailCode",
                        DisplayName = "MailCountryName",
                        Provinces =
                        [
                            new Province
                            {
                                Name = "Zuid-Holland"
                            }
                        ]
                    },
                    NotExposed = "MailNotExposed"
                },
                Addresses =
                [
                    new Address
                    {
                        Street = "Street1",
                        PostalCode = "PostalCode1",
                        Country = new Country
                        {
                            Code = "ESP",
                            DisplayName = "Spain"
                        },
                        NotExposed = "NotExposed1"
                    },
                    new Address
                    {
                        Street = "Street2",
                        PostalCode = "PostalCode2",
                        Country = new Country
                        {
                            Code = "Country2",
                            DisplayName = "CountryName2"
                        },
                        NotExposed = "NotExposed2"
                    }
                ],
                NamesOfChildren =
                [
                    "John",
                    "Jack",
                    "Joe",
                    null
                ],
                AgesOfChildren =
                [
                    10,
                    20,
                    30,
                    null
                ]
            }
        }
        /*, new Book
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
        }*/
    );
#pragma warning restore format

    await dbContext.SaveChangesAsync();
}
