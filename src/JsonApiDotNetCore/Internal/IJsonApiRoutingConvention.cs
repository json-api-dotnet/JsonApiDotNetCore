// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Internal
{
    public interface IJsonApiRoutingConvention : IApplicationModelConvention
    {
    }
}
