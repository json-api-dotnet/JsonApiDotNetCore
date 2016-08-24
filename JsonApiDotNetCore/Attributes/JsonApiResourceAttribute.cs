using System;

namespace JsonApiDotNetCore.Attributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class JsonApiResourceAttribute : Attribute
  {
    public Type JsonApiResourceType { get; set; }

    public JsonApiResourceAttribute(Type jsonApiResourceType)
    {
      JsonApiResourceType = jsonApiResourceType;
    }
  }
}
