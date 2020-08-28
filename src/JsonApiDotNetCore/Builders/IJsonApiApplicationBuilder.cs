using System;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Builders
{
    internal interface IJsonApiApplicationBuilder
    {
        public Action<MvcOptions> ConfigureMvcOptions { get; set; }
    }
}
