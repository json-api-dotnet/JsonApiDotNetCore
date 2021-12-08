using JetBrains.Annotations;

namespace SourceGeneratorDebugger.Controllers;

// Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
public partial class ArticlesController
{
}

[PublicAPI]
partial class ArticlesController
{
    public void ExtraMethod()
    {
    }
}
