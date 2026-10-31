using FluentAssertions;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

internal static class WorkItemCompoundExtensions
{
    public static WorkItem WithCompounds(this WorkItem workItem)
    {
        workItem.Estimate = new TimeEstimate
        {
            Optimistic = TimeSpan.FromHours(2),
            Likely = TimeSpan.FromHours(4),
            Pessimistic = TimeSpan.FromHours(12)
        };

        workItem.Checklist =
        [
            new ChecklistItem
            {
                Text = "Write code",
                IsDone = true
            },
            new ChecklistItem
            {
                Text = "Write tests",
                IsDone = false
            }
        ];

        return workItem;
    }

    public static void ShouldHaveOriginalCompoundsOf(this WorkItem workItemInDatabase, WorkItem original)
    {
        workItemInDatabase.Estimate.Should().NotBeNull();
        workItemInDatabase.Estimate.Optimistic.Should().Be(original.Estimate!.Optimistic);
        workItemInDatabase.Estimate.Likely.Should().Be(original.Estimate.Likely);
        workItemInDatabase.Estimate.Pessimistic.Should().Be(original.Estimate.Pessimistic);

        workItemInDatabase.Checklist.Should().HaveCount(original.Checklist.Count);

        for (int index = 0; index < original.Checklist.Count; index++)
        {
            workItemInDatabase.Checklist[index].Text.Should().Be(original.Checklist[index].Text);
            workItemInDatabase.Checklist[index].IsDone.Should().Be(original.Checklist[index].IsDone);
        }
    }
}
