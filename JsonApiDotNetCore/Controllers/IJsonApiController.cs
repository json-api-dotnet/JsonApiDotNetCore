using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
  public interface IJsonApiController
  {
    ObjectResult Delete(string id);
    ObjectResult Get();
    ObjectResult Get(string id);
    ObjectResult Patch(string id, Dictionary<PropertyInfo, object> entityPatch);
    ObjectResult Post(object entity);
  }
}