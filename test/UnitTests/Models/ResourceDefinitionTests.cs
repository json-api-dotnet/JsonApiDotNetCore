using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.Models
{
    public sealed class ResourceDefinition_Scenario_Tests
    {
        [Fact]
        public void Property_Sort_Order_Uses_NewExpression()
        {
            // Arrange
            var resource = new RequestFilteredResource(isAdmin: false);

            // Act
            var sorts = resource.DefaultSort();

            // Assert
            Assert.Equal(2, sorts.Count);

            Assert.Equal(nameof(Model.CreatedAt), sorts[0].Attribute.PropertyInfo.Name);
            Assert.Equal(SortDirection.Ascending, sorts[0].SortDirection);

            Assert.Equal(nameof(Model.Password), sorts[1].Attribute.PropertyInfo.Name);
            Assert.Equal(SortDirection.Descending, sorts[1].SortDirection);
        }

        [Fact]
        public void Request_Filter_Uses_Member_Expression()
        {
            // Arrange
            var resource = new RequestFilteredResource(isAdmin: true);

            // Act
            var attrs = resource.GetAllowedAttributes();

            // Assert
            Assert.DoesNotContain(attrs, a => a.PropertyInfo.Name == nameof(Model.AlwaysExcluded));
        }

        [Fact]
        public void Request_Filter_Uses_NewExpression()
        {
            // Arrange
            var resource = new RequestFilteredResource(isAdmin: false);

            // Act
            var attrs = resource.GetAllowedAttributes();

            // Assert
            Assert.DoesNotContain(attrs, a => a.PropertyInfo.Name == nameof(Model.AlwaysExcluded));
            Assert.DoesNotContain(attrs, a => a.PropertyInfo.Name == nameof(Model.Password));
        }
    }

    public class Model : Identifiable
    {
        [Attr] public string AlwaysExcluded { get; set; }
        [Attr] public string Password { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
    }

    public sealed class RequestFilteredResource : ResourceDefinition<Model>
    {
        // this constructor will be resolved from the container
        // that means you can take on any dependency that is also defined in the container
        public RequestFilteredResource(bool isAdmin) : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).AddResource<Model>().Build())
        {
            if (isAdmin)
                HideFields(model => model.AlwaysExcluded);
            else
                HideFields(model => new { model.AlwaysExcluded, model.Password });
        }

        public override QueryFilters GetQueryFilters()
            => new QueryFilters {
                { "is-active", (query, value) => query.Select(x => x) }
            };

        public override PropertySortOrder GetDefaultSortOrder()
            => new PropertySortOrder
            {
                (model => model.CreatedAt, SortDirection.Ascending),
                (model => model.Password, SortDirection.Descending)
            };
    }
}
