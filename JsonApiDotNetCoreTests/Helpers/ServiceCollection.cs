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
  }
}
