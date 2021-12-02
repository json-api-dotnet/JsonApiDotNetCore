using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    public sealed class SerializerIgnoreConditionTests : IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>
    {
        private readonly QueryStringFakers _fakers = new();

        public SerializerIgnoreConditionTests()
        {
            UseController<CalendarsController>();
        }

        [Theory]
        [InlineData(JsonIgnoreCondition.Never, true, true)]
        [InlineData(JsonIgnoreCondition.WhenWritingDefault, false, false)]
        [InlineData(JsonIgnoreCondition.WhenWritingNull, false, true)]
        public async Task Applies_configuration_for_ignore_condition(JsonIgnoreCondition configurationValue, bool expectNullValueInDocument,
            bool expectDefaultValueInDocument)
        {
            // Arrange
            var options = (JsonApiOptions)Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.SerializerOptions.DefaultIgnoreCondition = configurationValue;

            Calendar calendar = _fakers.Calendar.Generate();
            calendar.TimeZone = null;
            calendar.DefaultAppointmentDurationInMinutes = default;
            calendar.ShowWeekNumbers = true;
            calendar.Appointments = _fakers.Appointment.Generate(1).ToHashSet();
            calendar.Appointments.Single().Description = null;
            calendar.Appointments.Single().StartTime = default;
            calendar.Appointments.Single().EndTime = 1.January(2001).AsUtc();

            await RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Calendars.Add(calendar);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/calendars/{calendar.StringId}?include=appointments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Included.ShouldHaveCount(1);

            if (expectNullValueInDocument)
            {
                responseDocument.Data.SingleValue.Attributes.ShouldContainKey("timeZone");
                responseDocument.Included[0].Attributes.ShouldContainKey("description");
            }
            else
            {
                responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("timeZone");
                responseDocument.Included[0].Attributes.Should().NotContainKey("description");
            }

            if (expectDefaultValueInDocument)
            {
                responseDocument.Data.SingleValue.Attributes.ShouldContainKey("defaultAppointmentDurationInMinutes");
                responseDocument.Included[0].Attributes.ShouldContainKey("startTime");
            }
            else
            {
                responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("defaultAppointmentDurationInMinutes");
                responseDocument.Included[0].Attributes.Should().NotContainKey("startTime");
            }
        }
    }
}
