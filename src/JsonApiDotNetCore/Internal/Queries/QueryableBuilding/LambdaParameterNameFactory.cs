using System;
using System.Collections.Generic;
using Humanizer;

namespace JsonApiDotNetCore.Internal.Queries.QueryableBuilding
{
    /// <summary>
    /// Produces unique names for lambda parameters.
    /// </summary>
    public sealed class LambdaParameterNameFactory
    {
        private readonly HashSet<string> _namesInScope = new HashSet<string>();

        public LambdaParameterNameScope Create(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            string parameterName = typeName.Camelize();
            parameterName = EnsureNameIsUnique(parameterName);

            _namesInScope.Add(parameterName);
            return new LambdaParameterNameScope(parameterName, this);
        }

        private string EnsureNameIsUnique(string name)
        {
            if (!_namesInScope.Contains(name))
            {
                return name;
            }

            int counter = 1;
            string alternativeName;

            do
            {
                counter++;
                alternativeName = name + counter;
            }
            while (_namesInScope.Contains(alternativeName));

            return alternativeName;
        }

        public void Release(string parameterName)
        {
            _namesInScope.Remove(parameterName);
        }
    }
}
