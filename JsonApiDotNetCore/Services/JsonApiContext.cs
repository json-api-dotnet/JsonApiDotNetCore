using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using JsonApiDotNetCore.Controllers;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiContext
  {
    private object _dbContext;
    private Route _route;

    public JsonApiContext(Route route, object dbContext)
    {
      _route = route;
      _dbContext = dbContext;
    }

    public List<object> Get()
    {
      return (GetDbSetFromContext(_route.ContextPropertyName) as IEnumerable<object>).ToList();
    }

    private object GetDbSetFromContext(string propName)
    {
      return _dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name == propName).GetValue(_dbContext, null);
    }
  }
}
