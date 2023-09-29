namespace TestBuildingBlocks;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
