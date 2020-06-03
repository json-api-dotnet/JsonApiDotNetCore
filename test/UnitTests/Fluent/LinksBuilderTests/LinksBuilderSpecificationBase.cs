using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;

namespace UnitTests.Fluent.LinksBuilderTests
{
    public abstract class LinksBuilderSpecificationBase : SpecificationBase
    {
        protected ResourceContext _resourceContext;
        protected JsonApiOptions _options;        
        protected LinksAttribute _linksAttribute;
        protected Link _topLevelLinks;
        protected Link _resourceLinks;
        protected Link _relationshipLinks;        

        protected override async Task Given()
        {
            await base.Given();

            _options = new JsonApiOptions();

            _resourceContext = new ResourceContext();

            _topLevelLinks = Link.All;
            _resourceLinks = Link.All;
            _relationshipLinks = Link.All;
        }      
    }
}
