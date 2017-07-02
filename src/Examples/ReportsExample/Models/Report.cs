using JsonApiDotNetCore.Models;

public class Report : Identifiable
{
    [Attr("title")]
    public string Title { get; set; }
    
    [Attr("complex-type")]
    public ComplexType ComplexType { get; set; }
}

public class ComplexType
{
    public string CompoundPropertyName { get; set; }
}