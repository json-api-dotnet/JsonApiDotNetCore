using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
  public class PersonResource : IJsonApiResource
  {
    public string Id { get; set; }
    public string Name { get; set; }
  }
}
