using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Specifications;
using Xunit;

namespace UnitTests.Internal.ComparersTests
{
    public class When_Using_RelationshipAttributeComparer: ComparerSpecificationBase
    {        
        RelationshipAttribute _newAttribute;
        RelationshipAttribute _exisitingAttribute;
        IEnumerable<RelationshipAttribute> _distinctResult;

        protected override async Task Given()
        {
            await base.Given();

            _exisitingAttribute = _resourceContext.Relationships.Where(x => x.PropertyInfo.Name == "Image")
                                                                .SingleOrDefault() as RelationshipAttribute;

            _newAttribute = new HasOneAttribute();
            _newAttribute.PropertyInfo = _exisitingAttribute.PropertyInfo;
            _newAttribute.PublicRelationshipName = "image";
        }

        protected override async Task When()
        {
            await base.When();

            List<RelationshipAttribute> attributes = new List<RelationshipAttribute>();
            attributes.Add(_exisitingAttribute);
            attributes.Add(_newAttribute);

            _distinctResult = attributes.Distinct(RelationshipAttributeComparer.Instance);
        }

        [Then]
        public void It_Should_Compare_By_PropertyInfo()
        {
            Assert.Single(_distinctResult);
        }
    }
}
