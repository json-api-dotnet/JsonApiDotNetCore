namespace JsonApiDotNetCore.Routing.Query
{
  public class SortParameter
  {
    public SortParameter(SortDirection direction, string propertyName)
    {
     Direction = direction;
     PropertyName = propertyName;
    }
    public SortDirection Direction { get; set; }
    public string PropertyName { get; set; }
  }
}
