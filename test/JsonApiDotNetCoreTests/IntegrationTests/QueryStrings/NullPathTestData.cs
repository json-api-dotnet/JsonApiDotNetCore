using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

/// <summary>
/// Seeds calendars and appointments whose collections (scalar and compound elements, root-level and nested in compounds) are null, empty or populated,
/// reachable through null compounds and null nested compounds. The last row of every set is the only one with populated collections.
/// </summary>
internal static class NullPathTestData
{
    public static readonly OfficeHours SampleHours = new()
    {
        OpensAt = new TimeOnly(9, 0),
        ClosesAt = new TimeOnly(17, 0)
    };

    /// <summary>
    /// Creates four calendars: 0 has no workweek, 1 has a workweek without nested compound and with null collections, 2 has a nested compound and empty
    /// collections (null in the nested compound), 3 has populated collections. Scalar collections nested in compounds are only null when requested,
    /// because\r\n    /// a stored JSON null cannot be queried for them.
    /// </summary>
    public static List<Calendar> CreateCalendars(QueryStringFakers fakers, bool nullScalarCollections = false)
    {
        List<Calendar> calendars = fakers.Calendar.GenerateList(4);

        calendars[0].Workweek = null;

        calendars[1].Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            ExtraDays = nullScalarCollections ? null : [],
            ExtraBreaks = null,
            Overtime = null
        };

        calendars[2].Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            ExtraDays = [],
            ExtraBreaks = [],
            Overtime = new OvertimePolicy
            {
                Days = nullScalarCollections ? null : [],
                Windows = null
            }
        };

        calendars[3].Workweek = new Workweek
        {
            FirstDay = DayOfWeek.Monday,
            ExtraDays = [DayOfWeek.Tuesday],
            ExtraBreaks = [SampleHours],
            Overtime = new OvertimePolicy
            {
                Days = [DayOfWeek.Monday],
                Windows = [SampleHours]
            }
        };

        return calendars;
    }

    /// <summary>
    /// Creates four appointments: 0 has null collections, 1 has empty collections, 2 has empty collections as well, 3 has populated collections. The
    /// calendars are those of <see cref="CreateCalendars" />, in the same order.
    /// </summary>
    public static List<Appointment> CreateAppointments(QueryStringFakers fakers, IReadOnlyList<Calendar> calendars)
    {
        List<Appointment> appointments = fakers.Appointment.GenerateList(4);

        appointments[0].Keywords = null;
        appointments[0].WaitingList = null;

        appointments[1].Keywords = [];
        appointments[1].WaitingList = [];

        appointments[2].Keywords = [];
        appointments[2].WaitingList = [];

        appointments[3].Keywords = ["x"];

        appointments[3].WaitingList =
        [
            new Attendee
            {
                Name = "Alice"
            }
        ];

        for (int index = 0; index < appointments.Count; index++)
        {
            appointments[index].Calendar = calendars[index];
        }

        return appointments;
    }
}
