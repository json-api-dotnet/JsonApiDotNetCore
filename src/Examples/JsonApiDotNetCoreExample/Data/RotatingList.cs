namespace JsonApiDotNetCoreExample.Data;

internal abstract class RotatingList
{
    public static RotatingList<T> Create<T>(int count, Func<int, T> createElement)
    {
        List<T> elements = new();

        for (int index = 0; index < count; index++)
        {
            T element = createElement(index);
            elements.Add(element);
        }

        return new RotatingList<T>(elements);
    }
}

internal sealed class RotatingList<T>
{
    private int _index = -1;

    public IList<T> Elements { get; }

    public RotatingList(IList<T> elements)
    {
        Elements = elements;
    }

    public T GetNext()
    {
        _index++;
        return Elements[_index % Elements.Count];
    }
}
