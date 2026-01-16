using System.Diagnostics;
using System.Reflection;
using Bogus;
using Xunit;

namespace TestBuildingBlocks;

public static class FakerExtensions
{
    public static Faker<T> MakeDeterministic<T>(this Faker<T> faker, DateTime? systemTimeUtc = null)
        where T : class
    {
        int seed = GetFakerSeed();
        faker.UseSeed(seed);

        // Setting the system DateTime to kind Utc, so that faker calls like PastOffset() don't depend on the system time zone.
        // See https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.op_implicit#remarks
        faker.UseDateTimeReference(systemTimeUtc ?? IntegrationTest.DefaultDateTimeUtc.UtcDateTime);

        return faker;
    }

#pragma warning disable AV1008 // Class should not be static
    public static int GetFakerSeed()
#pragma warning restore AV1008 // Class should not be static
    {
        // The goal here is to have stable data over multiple test runs, but at the same time different data per test case.

        MethodBase testMethod = GetTestMethod();
        string testName = $"{testMethod.DeclaringType?.FullName}.{testMethod.Name}";

        return GetDeterministicHashCode(testName);
    }

    private static MethodBase GetTestMethod()
    {
        var stackTrace = new StackTrace();

        MethodBase? testMethod = stackTrace.GetFrames().Select(stackFrame => stackFrame.GetMethod()).FirstOrDefault(IsTestMethod);

        if (testMethod == null)
        {
            // If called after the first await statement, the test method is no longer on the stack,
            // but has been replaced with the compiler-generated async/await state machine.
            throw new InvalidOperationException("Fakers can only be used from within (the start of) a test method.");
        }

        return testMethod;
    }

    private static bool IsTestMethod(MethodBase? method)
    {
        if (method == null)
        {
            return false;
        }

        return method.GetCustomAttribute<FactAttribute>() != null || method.GetCustomAttribute<TheoryAttribute>() != null;
    }

    private static int GetDeterministicHashCode(string source)
    {
        // https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int index = 0; index < source.Length; index += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ source[index];

                if (index == source.Length - 1)
                {
                    break;
                }

                hash2 = ((hash2 << 5) + hash2) ^ source[index + 1];
            }

            return hash1 + hash2 * 1566083941;
        }
    }

    // The methods below exist so that a non-nullable return type is inferred.
    // The Bogus NuGet package is not annotated for nullable reference types.

    public static T GenerateOne<T>(this Faker<T> faker)
        where T : class
    {
        return faker.Generate();
    }

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    public static List<T> GenerateList<T>(this Faker<T> faker, int count)
        where T : class
    {
        return faker.Generate(count);
    }

    public static HashSet<T> GenerateSet<T>(this Faker<T> faker, int count)
        where T : class
    {
        return faker.Generate(count).ToHashSet();
    }

    public static HashSet<TOut> GenerateSet<TIn, TOut>(this Faker<TIn> faker, int count)
        where TOut : class
        where TIn : class, TOut
    {
        return faker.Generate(count).Cast<TOut>().ToHashSet();
    }
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
}
