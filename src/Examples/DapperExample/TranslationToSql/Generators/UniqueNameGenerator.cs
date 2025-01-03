namespace DapperExample.TranslationToSql.Generators;

internal abstract class UniqueNameGenerator
{
    private readonly string _prefix;
    private int _lastIndex;

    protected UniqueNameGenerator(string prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        _prefix = prefix;
    }

    public string GetNext()
    {
        return $"{_prefix}{++_lastIndex}";
    }

    public void Reset()
    {
        _lastIndex = 0;
    }
}
