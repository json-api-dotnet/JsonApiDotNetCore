using System;

namespace JsonApiDotNetCore.Internal.Queries.QueryableBuilding
{
    public sealed class LambdaParameterNameScope : IDisposable
    {
        private readonly LambdaParameterNameFactory _owner;

        public string Name { get; }

        public LambdaParameterNameScope(string name, LambdaParameterNameFactory owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public void Dispose()
        {
            _owner.Release(Name);
        }
    }
}
