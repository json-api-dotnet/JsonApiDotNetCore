using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.FireAndForgetDelivery
{
    public sealed partial class FireForgetTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<FireForgetDbContext>, FireForgetDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<FireForgetDbContext>, FireForgetDbContext> _testContext;
        private readonly DomainFakers _fakers = new DomainFakers();

        public FireForgetTests(ExampleIntegrationTestContext<TestableStartup<FireForgetDbContext>, FireForgetDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<DomainUsersController>();
            testContext.UseController<DomainGroupsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<FireForgetUserDefinition>();
                services.AddResourceDefinition<FireForgetGroupDefinition>();

                services.AddSingleton<MessageBroker>();
            });

            var messageBroker = _testContext.Factory.Services.GetRequiredService<MessageBroker>();
            messageBroker.Reset();
        }

        [Fact]
        public async Task Does_not_send_message_on_write_error()
        {
            // Arrange
            string missingUserId = Guid.NewGuid().ToString();

            string route = "/domainUsers/" + missingUserId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'domainUsers' with ID '{missingUserId}' does not exist.");

            var messageBroker = _testContext.Factory.Services.GetRequiredService<MessageBroker>();
            messageBroker.SentMessages.Should().BeEmpty();
        }

        [Fact]
        public async Task Does_not_rollback_on_message_delivery_error()
        {
            // Arrange
            var messageBroker = _testContext.Factory.Services.GetRequiredService<MessageBroker>();
            messageBroker.SimulateFailure = true;

            DomainUser existingUser = _fakers.DomainUser.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<DomainUser>();
                dbContext.Users.Add(existingUser);
                await dbContext.SaveChangesAsync();
            });

            string route = "/domainUsers/" + existingUser.StringId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            error.Title.Should().Be("Message delivery failed.");
            error.Detail.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                DomainUser user = await dbContext.Users.FirstWithIdOrDefaultAsync(existingUser.Id);
                user.Should().BeNull();
            });
        }
    }
}
