using System;

namespace JsonApiDotNetCore.Attributes
{
  /// <summary>
  /// Allows for explicit serialization definitions on the classes themselves
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class SerializationFormat : Attribute
  {
    public string SingluarName;
    public string PluralName;

    public SerializationFormat(string singular, string plural)
    {
      SingluarName = singular;
      PluralName = plural;
    }
  }
}
