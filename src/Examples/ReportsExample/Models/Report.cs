using JsonApiDotNetCore.Models;

public class Report : Identifiable
{
    [Attr]
    public string Title { get; set; }
    
    [Attr]
    public ComplexType ComplexType { get; set; }
}

public class ComplexType
{
    public string CompoundPropertyName { get; set; }
}