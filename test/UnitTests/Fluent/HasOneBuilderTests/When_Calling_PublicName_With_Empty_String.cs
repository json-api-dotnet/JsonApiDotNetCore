using JsonApiDotNetCore.Models;
using System;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.HasOneBuilderTests
{
    public sealed class When_Calling_PublicName_With_Empty_String : HasOneBuilderSpecificationBase
    {
        Exception _exception;

        protected override async Task Given()
        {
            await base.Given();

            SetupHasOneBuilderWithUnAnnotatedProperty();

            _hasOneBuilder.Build();
        }

        protected override async Task When()
        {
            await base.When();

            try
            {
                _hasOneBuilder.PublicName("");
            }
            catch (Exception ex)
            {
                _exception = ex;
            }            
        }

        [Then]
        public void It_Should_Throw_ArgumentException()
        {
            Assert.IsType<ArgumentException>(_exception);
        }
    }
}
