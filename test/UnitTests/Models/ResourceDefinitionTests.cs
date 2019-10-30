using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System.Linq;
using Xunit;

namespace UnitTests.Models
{
    public class ResourceDefinition_Scenario_Tests
    {
        private readonly IResourceGraph _resourceGraph;

        public ResourceDefinition_Scenario_Tests()
        {
            _resourceGraph = new ResourceGraphBuilder()
                .AddResource<Model>("models")
                .Build();
        }

        [Fact]
        public void Request_Filter_Uses_Member_Expression()
        {
            // Arrange
            var resource = new RequestFilteredResource(isAdmin: true);

            // Act
            var attrs = resource.GetAllowedAttributes();

            // Assert
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.AlwaysExcluded));
        }

        [Fact]
        public void Request_Filter_Uses_NewExpression()
        {
            // Arrange
            var resource = new RequestFilteredResource(isAdmin: false);

            // Act
            var attrs = resource.GetAllowedAttributes();

            // Assert
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.AlwaysExcluded));
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.Password));
        }
    }

    public class Model : Identifiable
    {
        [Attr("name")] public string AlwaysExcluded { get; set; }
        [Attr("password")] public string Password { get; set; }
        [Attr("prop")] public string Prop { get; set; }
    }

    public class RequestFilteredResource : ResourceDefinition<Model>
    {
        // this constructor will be resolved from the container
        // that means you can take on any dependency that is also defined in the container
        public RequestFilteredResource(bool isAdmin) : base(new ResourceGraphBuilder().AddResource<Model>().Build())
        {
            if (isAdmin)
                HideFields(m => m.AlwaysExcluded);
            else
                HideFields(m => new { m.AlwaysExcluded, m.Password });
        }

        public override QueryFilters GetQueryFilters()
            => new QueryFilters {
                { "is-active", (query, value) => query.Select(x => x) }
            };
        public override PropertySortOrder GetDefaultSortOrder()
            => new PropertySortOrder {
                (t => t.Prop, SortDirection.Ascending)
            };
    }
}
