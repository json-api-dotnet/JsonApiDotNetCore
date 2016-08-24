using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Abstractions;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiContext
  {
    private readonly object _dbContext;
    public Route Route;

    public JsonApiContext(Route route, object dbContext)
    {
      Route = route;
      _dbContext = dbContext;
    }

    public List<object> Get()
    {
      return (GetDbSetFromContext(Route.ContextPropertyName) as IEnumerable<object>)?.ToList();
    }

    public object Get(string id)
    {
      return (GetDbSetFromContext(Route.ContextPropertyName) as IQueryable<IModel>)?.FirstOrDefault(x => x.Id.ToString() == id);
    }

    private object GetDbSetFromContext(string propName)
    {
      return _dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name == propName)?.GetValue(_dbContext, null);
    }
  }
}
