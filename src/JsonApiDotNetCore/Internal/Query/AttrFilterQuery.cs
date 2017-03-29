using System;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
  public class AttrFilterQuery
  {
    private readonly IJsonApiContext _jsonApiContext;

    public AttrFilterQuery(
        IJsonApiContext jsonApiCopntext,
        FilterQuery filterQuery)
    {
      _jsonApiContext = jsonApiCopntext;

      var attribute = GetAttribute(filterQuery.Key);

      if (attribute == null)
        throw new JsonApiException("400", $"{filterQuery.Key} is not a valid property.");

      FilteredAttribute = attribute;
      PropertyValue = filterQuery.Value;
      FilterOperation = GetFilterOperation(filterQuery.Operation);
    }

    public AttrAttribute FilteredAttribute { get; set; }
    public string PropertyValue { get; set; }
    public FilterOperations FilterOperation { get; set; }

    private FilterOperations GetFilterOperation(string prefix)
    {
      if (prefix.Length == 0) return FilterOperations.eq;

      FilterOperations opertion;
      if (!Enum.TryParse<FilterOperations>(prefix, out opertion))
        throw new JsonApiException("400", $"Invalid filter prefix '{prefix}'");

      return opertion;
    }

    private AttrAttribute GetAttribute(string propertyName)
    {
      return _jsonApiContext.RequestEntity.Attributes
          .FirstOrDefault(attr =>
              attr.InternalAttributeName.ToLower() == propertyName.ToLower()
      );
    }
  }
}