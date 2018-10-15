using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    /// <inheritdoc />
    [Obsolete("Use IResourceGraph instead.")]
    public interface IContextGraph : IResourceGraph { }

    [Obsolete("Use ResourceGraph instead.")]
    public class ContextGraph : ResourceGraph { }
}
