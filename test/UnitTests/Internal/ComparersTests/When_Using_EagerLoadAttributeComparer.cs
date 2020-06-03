using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Specifications;
using Xunit;

namespace UnitTests.Internal.ComparersTests
{
    public class When_Using_EagerLoadAttributeComparer: ComparerSpecificationBase
    {        
        EagerLoadAttribute _newAttribute;
        EagerLoadAttribute _exisitingAttribute;
        IEnumerable<EagerLoadAttribute> _distinctResult;

        protected override async Task Given()
        {
            await base.Given();
            
            _exisitingAttribute = _resourceContext.EagerLoads.Where(x => x.Property.Name == "UnitPrice")
                                                             .SingleOrDefault() as EagerLoadAttribute;

            _newAttribute = new EagerLoadAttribute();
            _newAttribute.Property = _exisitingAttribute.Property;            
        }

        protected override async Task When()
        {
            await base.When();

            List<EagerLoadAttribute> attributes = new List<EagerLoadAttribute>();
            attributes.Add(_exisitingAttribute);
            attributes.Add(_newAttribute);

            _distinctResult = attributes.Distinct(EagerLoadAttributeComparer.Instance);
        }

        [Then]
        public void It_Should_Compare_By_PropertyInfo()
        {
            Assert.Single(_distinctResult);
        }
    }
}
