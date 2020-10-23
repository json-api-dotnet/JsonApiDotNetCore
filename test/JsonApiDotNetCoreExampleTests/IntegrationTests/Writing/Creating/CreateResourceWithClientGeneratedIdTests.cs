using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing.Creating
{
    public sealed class CreateResourceWithClientGeneratedIdTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;

        public CreateResourceWithClientGeneratedIdTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects()
        {
            // Arrange
            var group = WriteFakers.WorkItemGroup.Generate();
            group.Id = Guid.NewGuid();

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = group.StringId,
                    attributes = new
                    {
                        name = group.Name
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Id.Should().Be(group.StringId);
            responseDocument.SingleData.Attributes["name"].Should().Be(group.Name);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups.ToListAsync();

                var newGroupInDatabase = groupsInDatabase.Single(p => p.Id == group.Id);
                newGroupInDatabase.Name.Should().Be(group.Name);
            });

            var property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(Guid));
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects_with_fieldset()
        {
            // Arrange
            var group = WriteFakers.WorkItemGroup.Generate();
            group.Id = Guid.NewGuid();

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = group.StringId,
                    attributes = new
                    {
                        name = group.Name
                    }
                }
            };

            var route = "/workItemGroups?fields=name";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Id.Should().Be(group.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["name"].Should().Be(group.Name);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups.ToListAsync();

                var newGroupInDatabase = groupsInDatabase.Single(p => p.Id == group.Id);
                newGroupInDatabase.Name.Should().Be(group.Name);
            });

            var property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(Guid));
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
        {
            // Arrange
            var color = new RgbColor
            {
                Id = "#FF0000",
                DisplayName = "Red"
            };
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<RgbColor>();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = color.StringId,
                    attributes = new
                    {
                        displayName = color.DisplayName
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorsInDatabase = await dbContext.RgbColors.ToListAsync();

                var newColorInDatabase = colorsInDatabase.Single(p => p.Id == color.Id);
                newColorInDatabase.DisplayName.Should().Be(color.DisplayName);
            });

            var property = typeof(RgbColor).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Cannot_create_resource_for_existing_client_generated_ID()
        {
            // Arrange
            var existingColor = WriteFakers.RgbColor.Generate();
            existingColor.Id = "#FFFFFF";

            var colorToCreate = WriteFakers.RgbColor.Generate();
            colorToCreate.Id = existingColor.Id;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<RgbColor>();
                dbContext.RgbColors.Add(existingColor);
                
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = colorToCreate.StringId,
                    attributes = new
                    {
                        displayName = colorToCreate.DisplayName
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            // TODO: Produce a better error (409:Conflict) and assert on its details here.
            responseDocument.Errors.Should().HaveCount(1);
        }
    }
}
