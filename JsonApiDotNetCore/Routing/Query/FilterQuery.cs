namespace JsonApiDotNetCore.Routing.Query
{
  public class FilterQuery
  {
    public FilterQuery(string propertyName, string propertyValue)
    {
      PropertyName = propertyName;
      PropertyValue = propertyValue;
    }
    public string PropertyName { get; set; }
    public string PropertyValue { get; set; }
  }
}
