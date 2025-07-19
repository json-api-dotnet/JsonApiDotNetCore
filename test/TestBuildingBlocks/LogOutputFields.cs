using JetBrains.Annotations;

namespace TestBuildingBlocks;

[PublicAPI]
[Flags]
public enum LogOutputFields
{
    None = 0,
    Level = 1,
    CategoryName = 1 << 1,
    CategoryNamespace = 1 << 2,
    Message = 1 << 3,
    Exception = 1 << 4,
    Scopes = 1 << 5,

    Category = CategoryName | CategoryNamespace,
    All = Level | Category | Message | Exception | Scopes
}
