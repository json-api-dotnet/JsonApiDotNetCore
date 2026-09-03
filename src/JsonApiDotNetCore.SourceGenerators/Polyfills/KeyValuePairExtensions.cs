using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
// ReSharper disable CheckNamespace

namespace System.Collections.Generic;

[ExcludeFromCodeCoverage]
internal static class KeyValuePairExtensions
{
    /// <summary>
    /// Deconstructs the current KeyValuePair into its key and value.
    /// </summary>
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> target, out TKey key, out TValue value)
    {
        key = target.Key;
        value = target.Value;
    }
}
