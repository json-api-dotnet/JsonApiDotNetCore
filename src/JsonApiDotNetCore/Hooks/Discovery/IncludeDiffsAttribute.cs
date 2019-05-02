using System;
namespace JsonApiDotNetCore.Hooks.Discovery
{
    public class DatabaseValuesInDiffs : Attribute
    {
        public readonly bool IcludeDatabaseValues;
        public DatabaseValuesInDiffs(bool includeDatabaseValues = true)
        {
            IcludeDatabaseValues = includeDatabaseValues;
        }
    }
}
