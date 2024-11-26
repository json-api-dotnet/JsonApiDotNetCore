using System.Collections;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore;

internal sealed class CollectionConverter
{
    private static readonly HashSet<Type> HashSetCompatibleCollectionTypes =
    [
        typeof(HashSet<>),
        typeof(ISet<>),
        typeof(IReadOnlySet<>),
        typeof(ICollection<>),
        typeof(IReadOnlyCollection<>),
        typeof(IEnumerable<>)
    ];

    public static CollectionConverter Instance { get; } = new();

    private CollectionConverter()
    {
    }

    /// <summary>
    /// Creates a collection instance based on the specified collection type and copies the specified elements into it.
    /// </summary>
    /// <param name="source">
    /// Source to copy from.
    /// </param>
    /// <param name="collectionType">
    /// Target collection type, for example: <code><![CDATA[
    /// typeof(List<Article>)
    /// ]]></code> or <code><![CDATA[
    /// typeof(ISet<Person>)
    /// ]]></code>.
    /// </param>
    public IEnumerable CopyToTypedCollection(IEnumerable source, Type collectionType)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(collectionType);

        Type concreteCollectionType = ToConcreteCollectionType(collectionType);
        dynamic concreteCollectionInstance = Activator.CreateInstance(concreteCollectionType)!;

        foreach (object item in source)
        {
            concreteCollectionInstance.Add((dynamic)item);
        }

        return concreteCollectionInstance;
    }

    /// <summary>
    /// Returns a compatible collection type that can be instantiated, for example: <code><![CDATA[
    /// IList<Article> -> List<Article>
    /// ]]></code> or
    /// <code><![CDATA[
    /// ISet<Article> -> HashSet<Article>
    /// ]]></code>.
    /// </summary>
    private Type ToConcreteCollectionType(Type collectionType)
    {
        if (collectionType is { IsInterface: true, IsGenericType: true })
        {
            Type openCollectionType = collectionType.GetGenericTypeDefinition();

            if (HashSetCompatibleCollectionTypes.Contains(openCollectionType))
            {
                return typeof(HashSet<>).MakeGenericType(collectionType.GenericTypeArguments[0]);
            }

            if (openCollectionType == typeof(IList<>) || openCollectionType == typeof(IReadOnlyList<>))
            {
                return typeof(List<>).MakeGenericType(collectionType.GenericTypeArguments[0]);
            }
        }

        return collectionType;
    }

    /// <summary>
    /// Returns a collection that contains zero, one or multiple resources, depending on the specified value.
    /// </summary>
    public IReadOnlyCollection<IIdentifiable> ExtractResources(object? value)
    {
        return value switch
        {
            List<IIdentifiable> resourceList => resourceList.AsReadOnly(),
            HashSet<IIdentifiable> resourceSet => resourceSet.AsReadOnly(),
            IReadOnlyCollection<IIdentifiable> resourceCollection => resourceCollection,
            IEnumerable<IIdentifiable> resources => resources.ToArray().AsReadOnly(),
            IIdentifiable resource => [resource],
            _ => Array.Empty<IIdentifiable>()
        };
    }

    /// <summary>
    /// Returns the element type if the specified type is a generic collection, for example: <code><![CDATA[
    /// IList<string> -> string
    /// ]]></code> or
    /// <code><![CDATA[
    /// IList -> null
    /// ]]></code>.
    /// </summary>
    public Type? FindCollectionElementType(Type? type)
    {
        if (type is { IsGenericType: true, GenericTypeArguments.Length: 1 })
        {
            if (type.IsOrImplementsInterface<IEnumerable>())
            {
                return type.GenericTypeArguments[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Indicates whether a <see cref="HashSet{T}" /> instance can be assigned to the specified type, for example:
    /// <code><![CDATA[
    /// IList<Article> -> false
    /// ]]></code> or <code><![CDATA[
    /// ISet<Article> -> true
    /// ]]></code>.
    /// </summary>
    public bool TypeCanContainHashSet(Type collectionType)
    {
        ArgumentGuard.NotNull(collectionType);

        if (collectionType.IsGenericType)
        {
            Type openCollectionType = collectionType.GetGenericTypeDefinition();
            return HashSetCompatibleCollectionTypes.Contains(openCollectionType);
        }

        return false;
    }
}
