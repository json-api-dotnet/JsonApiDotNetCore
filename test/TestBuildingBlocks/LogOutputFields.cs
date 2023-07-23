using JetBrains.Annotations;

namespace TestBuildingBlocks;

[PublicAPI]
[Flags]
public enum LogOutputFields
{
    None = 0,
    Level = 1,
    Category = 1 << 1,
    Message = 1 << 2,
    Exception = 1 << 3,
    Scopes = 1 << 4,

    All = Level | Category | Message | Exception | Scopes
}
