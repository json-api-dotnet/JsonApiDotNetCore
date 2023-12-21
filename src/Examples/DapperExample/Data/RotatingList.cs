namespace DapperExample.Data;

internal abstract class RotatingList
{
    public static RotatingList<T> Create<T>(int count, Func<int, T> createElement)
    {
        List<T> elements = [];

        for (int index = 0; index < count; index++)
        {
            T element = createElement(index);
            elements.Add(element);
        }

        return new RotatingList<T>(elements);
    }
}

internal sealed class RotatingList<T>(IList<T> elements)
{
    private int _index = -1;

    public IList<T> Elements { get; } = elements;

    public T GetNext()
    {
        _index++;
        return Elements[_index % Elements.Count];
    }
}
