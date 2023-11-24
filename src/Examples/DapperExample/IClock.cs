namespace DapperExample;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
