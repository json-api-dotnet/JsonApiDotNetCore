using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Specifications;
using Xunit;

namespace UnitTests.Internal.ComparersTests
{
    public class When_Using_AttrAttributeComparer : ComparerSpecificationBase
    {
        AttrAttribute _newAttribute;
        AttrAttribute _exisitingAttribute;
        IEnumerable<AttrAttribute> _distinctResult;

        protected override async Task Given()
        {
            await base.Given();
            
            _exisitingAttribute = _resourceContext.Attributes.Where(x => x.PropertyInfo.Name == "Name")
                                                             .SingleOrDefault() as AttrAttribute;

            _newAttribute = new AttrAttribute();
            _newAttribute.PropertyInfo = _exisitingAttribute.PropertyInfo;
            _newAttribute.PublicAttributeName = "name";
            _newAttribute.Capabilities = _options.DefaultAttrCapabilities;
        }

        protected override async Task When()
        {
            await base.When();

            List<AttrAttribute> attributes = new List<AttrAttribute>();
            attributes.Add(_exisitingAttribute);
            attributes.Add(_newAttribute);

            _distinctResult = attributes.Distinct(AttrAttributeComparer.Instance);
        }

        [Then]
        public void It_Should_Compare_By_PropertyInfo()
        {
            Assert.Single(_distinctResult);
        }
    }
}
