namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Represents the name and value of a parameter. For example: <code><![CDATA[
/// @p1 = 'Yes'
/// ]]></code>.
/// </summary>
internal sealed class ParameterNode : SqlValueNode
{
    private static readonly ParameterFormatter Formatter = new();

    public string Name { get; }
    public object? Value { get; }

    public ParameterNode(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!name.StartsWith('@') || name.Length < 2)
        {
            throw new ArgumentException("Parameter name must start with an '@' symbol and not be empty.", nameof(name));
        }

        Name = name;
        Value = value;
    }

    public override TResult Accept<TArgument, TResult>(SqlTreeNodeVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitParameter(this, argument);
    }

    public override string ToString()
    {
        return Formatter.Format(Name, Value);
    }
}
