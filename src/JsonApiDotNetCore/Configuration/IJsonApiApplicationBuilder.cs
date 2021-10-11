using System;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Configuration
{
    internal interface IJsonApiApplicationBuilder
    {
        public Action<MvcOptions>? ConfigureMvcOptions { set; }
    }
}
