public interface IJsonApiModelConfiguration
{
  void UseContext<T>();
  void SetDefaultNamespace(string ns);
}
