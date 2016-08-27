using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreTests.Helpers
{
  public class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
  {
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public bool ContainsType(Type type)
    {
      var ret = false;
      this.ForEach(sD => {
        if(sD.ServiceType == type)
        {
          ret = true;
        }
      });
      return ret;
    }
  }
}
