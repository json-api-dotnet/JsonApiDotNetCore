using System;
using Microsoft.EntityFrameworkCore;

public interface IJsonApiModelConfiguration
{
  void UseContext<T>();
  void SetDefaultNamespace(string ns);
}
