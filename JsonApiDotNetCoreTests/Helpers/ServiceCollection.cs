using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreTests
{
  public class ServiceCollection : IServiceCollection
  {
    public int IndexOf(ServiceDescriptor serviceDescriptor)
    {
      throw new NotImplementedException();
    }

    public void Insert(int pos, ServiceDescriptor serviceDescriptor)
    {
      throw new NotImplementedException();
    }

    public void RemoveAt(int pos)
    {
      throw new NotImplementedException();
    }

    public bool Remove(ServiceDescriptor serviceDescriptor)
    {
      throw new NotImplementedException();
    }

    public void Add(ServiceDescriptor serviceDescriptor)
    {
      throw new NotImplementedException();
    }

    public void Clear()
    {
      throw new NotImplementedException();
    }

    public bool Contains(ServiceDescriptor serviceDescriptor)
    {
      throw new NotImplementedException();
    }

    public void CopyTo(ServiceDescriptor[] serviceDescriptor, int pos)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<ServiceDescriptor>.IEnumerator<ServiceDescriptor> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public int Count { get; set; }
    public bool IsReadOnly { get; set; }

  }
}
