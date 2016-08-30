using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Controllers
{
  public interface IJsonApiController
  {
    ObjectResult Get();
    ObjectResult Get(string id);
    ObjectResult Post(object entity);
    ObjectResult Patch(string id, Dictionary<PropertyInfo, object> entityPatch);
    ObjectResult Delete(string id);
  }
}
