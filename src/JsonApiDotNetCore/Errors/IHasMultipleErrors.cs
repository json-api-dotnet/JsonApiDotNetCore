using System.Collections.Generic;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    public interface IHasMultipleErrors
    {
        public IReadOnlyCollection<Error> Errors { get; }
    }
}
