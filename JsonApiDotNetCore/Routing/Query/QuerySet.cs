using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing.Query
{
  public class QuerySet
  {
    public QuerySet (IQueryCollection query)
    {
      BuildQuerySet(query);
    }
    public FilterQuery Filter { get; set; }
    public List<SortParameter> SortParameters { get; set; }

    private void BuildQuerySet(IQueryCollection query)
    {
      foreach (var pair in query)
      {
        if(pair.Key.StartsWith("filter"))
        {
          Filter = ParseFilterQuery(pair.Key, pair.Value);
          continue;
        }

        if(pair.Key.StartsWith("sort")){
          SortParameters = ParseSortParameters(pair.Value);
        }
      }
    }

    private FilterQuery ParseFilterQuery(string key, string value)
    {
      // expected input = filter[id]=1
      var propertyName = key.Split('[', ']')[1].ToProperCase();
      return new FilterQuery(propertyName, value);
    }

    // sort=id,name
    // sort=-id
    private List<SortParameter> ParseSortParameters(string value)
    {
      var sortParameters = new List<SortParameter>();
      value.Split(',').ToList().ForEach(p => {
        var direction = SortDirection.Ascending;
        if(p[0] == '-')
        {
          direction = SortDirection.Descending;
          p = p.Substring(1);
        }
        sortParameters.Add(new SortParameter(direction, p.ToProperCase()));
      });

      return sortParameters;
    }
  }
}
