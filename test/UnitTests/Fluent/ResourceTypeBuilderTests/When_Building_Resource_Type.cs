using JsonApiDotNetCore.Fluent;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Specifications;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Fluent.ResourceTypeBuilderTests
{
    public sealed class When_Building_Resource_Type: ResourceTypeBuilderSpecificationBase
    {        
        ResourceTypeBuilder<UnAnnotatedProduct> _resourceTypeBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceGraphBuilder.AddResource<UnAnnotatedProduct>();
            _resourceGraphBuilder.Build();

            _resourceContext = _resourceGraphBuilder.GetResourceContext(typeof(UnAnnotatedProduct));

            _resourceTypeBuilder = _resourceGraphBuilder.Resource<UnAnnotatedProduct>();
        }

        protected override async Task When()
        {
            await base.When();

            var builder = _resourceTypeBuilder;

            builder
                .ResourceName("products-catalog");

            builder
                .Links(Link.All, Link.All, Link.All);

            builder
                .Attribute(x => x.Name)
                .PublicName("product-name")
                .Capabilites(AttrCapabilities.All);

            builder
                .EagerLoad(x => x.UnitPrice);

            builder
                .HasOne(x => x.Image)                
                .InverseNavigation(x => x.Name)
                .PublicName("image")
                .CanInclude(true)                
                .PublicName("product-image")
                .CanInclude(true);
                

            builder
                .HasMany(x => x.Tags)
                .PublicName("product-tags");

            builder
                .HasManyThrough(x => x.Categories, y => y.ProductCategories)
                .PublicName("product-categories");                        
        }

        [Then]
        public void It_Should_Configure_ResourceName()
        {
            Assert.Equal("products-catalog", _resourceContext.ResourceName);
        }

        [Then]
        public void It_Should_Configure_Links()
        {
            Assert.Equal(Link.All, _resourceContext.TopLevelLinks);
            Assert.Equal(Link.All, _resourceContext.ResourceLinks);
            Assert.Equal(Link.All, _resourceContext.RelationshipLinks);
        }

        [Then]
        public void It_Should_Add_Attributes()
        {
            Assert.Equal(2, _resourceContext.Attributes.Count);

            AttrAttribute idAttribute = _resourceContext.Attributes.Where(x => x.PropertyInfo.Name == "Id")
                                                                   .SingleOrDefault();

            AttrAttribute nameAttribute = _resourceContext.Attributes.Where(x => x.PropertyInfo.Name == "Name")
                                                                     .SingleOrDefault();

            Assert.Equal("product-name", nameAttribute.PublicAttributeName);
        }

        [Then]
        public void It_Should_Add_EagerLoads()
        {
            Assert.Single(_resourceContext.EagerLoads);
        }

        [Then]
        public void It_Should_Add_Relationships()
        {
            Assert.Equal(3, _resourceContext.Relationships.Count);
        }
    }
}
