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
    public sealed class When_Building_Resource_Type_On_Annotated_Model : ResourceTypeBuilderSpecificationBase
    {        
        ResourceTypeBuilder<AnnotatedProduct> _resourceTypeBuilder;

        protected override async Task Given()
        {
            await base.Given();

            _resourceGraphBuilder.AddResource<AnnotatedProduct>();
            _resourceGraphBuilder.Build();

            _resourceContext = _resourceGraphBuilder.GetResourceContext(typeof(AnnotatedProduct));

            _resourceTypeBuilder = _resourceGraphBuilder.Resource<AnnotatedProduct>();
        }

        protected override async Task When()
        {
            await base.When();

            var builder = _resourceTypeBuilder;

            builder
                .ResourceName("products-catalog");
            
            builder
                .HasMany(x => x.Tags)
                .PublicName("product-tags");            
        }

        [Then]
        public void It_Should_Combine_Annotations_With_Fluent_Configurations()
        {
            Assert.Equal(Link.None, _resourceContext.TopLevelLinks);
            Assert.Equal(Link.None, _resourceContext.ResourceLinks);
            Assert.Equal(Link.None, _resourceContext.RelationshipLinks);

            Assert.Equal(2, _resourceContext.Attributes.Count);

            Assert.Single(_resourceContext.EagerLoads);

            Assert.Equal(3, _resourceContext.Relationships.Count);
        }

        [Then]
        public void It_Should_Override_Annotations_With_Fluent_Configurations()
        {
            Assert.Equal("products-catalog", _resourceContext.ResourceName);

            HasManyAttribute hasManyAttribute = _resourceContext.Relationships.Where(x => x.PropertyInfo.Name == "Tags")
                                                                              .SingleOrDefault() as HasManyAttribute;

            Assert.Equal("product-tags", hasManyAttribute.PublicRelationshipName);
        }
    }
}
