using System.Reflection;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

internal static class DatabaseFunctionStub
{
    public static readonly MethodInfo DecryptMethod = typeof(DatabaseFunctionStub).GetMethod(nameof(Decrypt), [typeof(string)])!;

    public static string Decrypt(string text)
    {
        _ = text;
        throw new InvalidOperationException($"The '{nameof(Decrypt)}' user-defined SQL function cannot be called client-side.");
    }
}
