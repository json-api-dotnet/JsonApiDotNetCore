using JetBrains.Annotations;

namespace NoEntityFrameworkExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public enum TodoItemPriority
{
    High = 1,
    Medium = 2,
    Low = 3
}
