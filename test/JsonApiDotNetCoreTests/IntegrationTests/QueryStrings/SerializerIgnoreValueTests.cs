using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    public sealed class SerializerIgnoreValueTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new();

        public SerializerIgnoreValueTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<CalendarsController>();
        }

        [Theory]
        [InlineData(NullValueHandling.Ignore, false)]
        [InlineData(NullValueHandling.Include, true)]
        public async Task Applies_configuration_for_nulls(NullValueHandling configurationValue, bool expectInDocument)
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.SerializerSettings.NullValueHandling = configurationValue;

            Calendar calendar = _fakers.Calendar.Generate();
            calendar.TimeZone = null;
            calendar.Appointments = _fakers.Appointment.Generate(1).ToHashSet();
            calendar.Appointments.Single().Title = null;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Calendars.Add(calendar);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/calendars/{calendar.StringId}?include=appointments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);

            if (expectInDocument)
            {
                responseDocument.SingleData.Attributes.Should().ContainKey("timeZone");
                responseDocument.Included[0].Attributes.Should().ContainKey("title");
            }
            else
            {
                responseDocument.SingleData.Attributes.Should().NotContainKey("timeZone");
                responseDocument.Included[0].Attributes.Should().NotContainKey("title");
            }
        }

        [Theory]
        [InlineData(DefaultValueHandling.Ignore, false)]
        [InlineData(DefaultValueHandling.Include, true)]
        public async Task Applies_configuration_for_defaults(DefaultValueHandling configurationValue, bool expectInDocument)
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.SerializerSettings.DefaultValueHandling = configurationValue;

            Calendar calendar = _fakers.Calendar.Generate();
            calendar.DefaultAppointmentDurationInMinutes = default;
            calendar.Appointments = _fakers.Appointment.Generate(1).ToHashSet();
            calendar.Appointments.Single().EndTime = default;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Calendars.Add(calendar);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/calendars/{calendar.StringId}?include=appointments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);

            if (expectInDocument)
            {
                responseDocument.SingleData.Attributes.Should().ContainKey("defaultAppointmentDurationInMinutes");
                responseDocument.Included[0].Attributes.Should().ContainKey("endTime");
            }
            else
            {
                responseDocument.SingleData.Attributes.Should().NotContainKey("defaultAppointmentDurationInMinutes");
                responseDocument.Included[0].Attributes.Should().NotContainKey("endTime");
            }
        }
    }
}
