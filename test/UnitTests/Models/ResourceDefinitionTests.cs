using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.Models
{
    public class ResourceDefinition_Scenario_Tests
    {
        private readonly IResourceGraph _graph;

        public ResourceDefinition_Scenario_Tests()
        {
            _graph = new ResourceGraphBuilder()
                .AddResource<Model>("models")
                .Build();
        }

        [Fact]
        public void Request_Filter_Uses_Member_Expression()
        {
            // arrange
            var resource = new RequestFilteredResource(isAdmin: true);

            // act
            var attrs = resource.GetOutputAttrs(null);

            // assert
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.AlwaysExcluded));
        }

        [Fact]
        public void Request_Filter_Uses_NewExpression()
        {
            // arrange
            var resource = new RequestFilteredResource(isAdmin: false);

            // act
            var attrs = resource.GetOutputAttrs(null);

            // assert
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.AlwaysExcluded));
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.Password));
        }

        [Fact]
        public void Instance_Filter_Uses_Member_Expression()
        {
            // arrange
            var model = new Model { AlwaysExcluded = "Admin" };
            var resource = new InstanceFilteredResource();

            // act
            var attrs = resource.GetOutputAttrs(model);

            // assert
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.AlwaysExcluded));
        }

        [Fact]
        public void Instance_Filter_Uses_NewExpression()
        {
            // arrange
            var model = new Model { AlwaysExcluded = "Joe" };
            var resource = new InstanceFilteredResource();

            // act
            var attrs = resource.GetOutputAttrs(model);

            // assert
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.AlwaysExcluded));
            Assert.DoesNotContain(attrs, a => a.InternalAttributeName == nameof(Model.Password));
        }

        [Fact]
        public void InstanceOutputAttrsAreSpecified_Returns_True_If_Instance_Method_Is_Overriden()
        {
            // act
            var resource = new InstanceFilteredResource();

            // assert
            Assert.True(resource._instanceAttrsAreSpecified);
        }
        
        [Fact]
        public void InstanceOutputAttrsAreSpecified_Returns_False_If_Instance_Method_Is_Not_Overriden()
        {
            // act
            var resource = new RequestFilteredResource(isAdmin: false);

            // assert
            Assert.False(resource._instanceAttrsAreSpecified);
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
        private readonly bool _isAdmin;

        // this constructor will be resolved from the container
        // that means you can take on any dependency that is also defined in the container
        public RequestFilteredResource(bool isAdmin)
        {
            _isAdmin = isAdmin;
        }

        // Called once per filtered resource in request.
        protected override List<AttrAttribute> OutputAttrs()
            => _isAdmin
                ? Remove(m => m.AlwaysExcluded)
                : Remove(m => new { m.AlwaysExcluded, m.Password }, from: base.OutputAttrs());
        
        public override QueryFilters GetQueryFilters()
            => new QueryFilters {
                { "is-active", (query, value) => query.Select(x => x) }
            };

        protected override PropertySortOrder GetDefaultSortOrder()
            => new PropertySortOrder {
                (t => t.Prop, SortDirection.Ascending)
            };
    }
    
    public class InstanceFilteredResource : ResourceDefinition<Model>
    {
        // Called once per resource instance
        protected override List<AttrAttribute> OutputAttrs(Model model)
            => model.AlwaysExcluded == "Admin"
                ? Remove(m => m.AlwaysExcluded, base.OutputAttrs())
                : Remove(m => new { m.AlwaysExcluded, m.Password }, from: base.OutputAttrs());
    }
}
