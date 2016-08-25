using System;

namespace JsonApiDotNetCore.Attributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class JsonApiIncludeAttribute : Attribute
  {
  }
}
