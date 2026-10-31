using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Queries;

public sealed class QueryExpressionTests
{
    public static IEnumerable<object[]> ExpressionTestData =>
        new QueryExpression[][]
        {
            [
                TestExpressionFactory.Instance.Any(),
                TestExpressionFactory.Instance.Any()
            ],
            [
                TestExpressionFactory.Instance.Comparison(),
                TestExpressionFactory.Instance.Comparison()
            ],
            [
                TestExpressionFactory.Instance.Count(),
                TestExpressionFactory.Instance.Count()
            ],
            [
                TestExpressionFactory.Instance.ScalarCollectionElementValue(),
                TestExpressionFactory.Instance.ScalarCollectionElementValue()
            ],
            [
                TestExpressionFactory.Instance.Has(),
                TestExpressionFactory.Instance.Has()
            ],
            [
                TestExpressionFactory.Instance.IncludeElement(),
                TestExpressionFactory.Instance.IncludeElement()
            ],
            [
                TestExpressionFactory.Instance.Include(),
                TestExpressionFactory.Instance.Include()
            ],
            [
                TestExpressionFactory.Instance.IsType(),
                TestExpressionFactory.Instance.IsType()
            ],
            [
                TestExpressionFactory.Instance.LiteralConstant(),
                TestExpressionFactory.Instance.LiteralConstant()
            ],
            [
                TestExpressionFactory.Instance.Logical(),
                TestExpressionFactory.Instance.Logical()
            ],
            [
                TestExpressionFactory.Instance.MatchText(),
                TestExpressionFactory.Instance.MatchText()
            ],
            [
                TestExpressionFactory.Instance.Not(),
                TestExpressionFactory.Instance.Not()
            ],
            [
                TestExpressionFactory.Instance.NullConstant(),
                TestExpressionFactory.Instance.NullConstant()
            ],
            [
                TestExpressionFactory.Instance.PaginationElementQueryStringValue(),
                TestExpressionFactory.Instance.PaginationElementQueryStringValue()
            ],
            [
                TestExpressionFactory.Instance.Pagination(),
                TestExpressionFactory.Instance.Pagination()
            ],
            [
                TestExpressionFactory.Instance.PaginationQueryStringValue(),
                TestExpressionFactory.Instance.PaginationQueryStringValue()
            ],
            [
                TestExpressionFactory.Instance.QueryableHandler(),
                TestExpressionFactory.Instance.QueryableHandler()
            ],
            [
                TestExpressionFactory.Instance.QueryStringParameterScope(),
                TestExpressionFactory.Instance.QueryStringParameterScope()
            ],
            [
                TestExpressionFactory.Instance.ResourceFieldChainForText(),
                TestExpressionFactory.Instance.ResourceFieldChainForText()
            ],
            [
                TestExpressionFactory.Instance.ResourceFieldChainForParent(),
                TestExpressionFactory.Instance.ResourceFieldChainForParent()
            ],
            [
                TestExpressionFactory.Instance.ResourceFieldChainForChildren(),
                TestExpressionFactory.Instance.ResourceFieldChainForChildren()
            ],
            [
                TestExpressionFactory.Instance.SortElement(),
                TestExpressionFactory.Instance.SortElement()
            ],
            [
                TestExpressionFactory.Instance.Sort(),
                TestExpressionFactory.Instance.Sort()
            ],
            [
                TestExpressionFactory.Instance.SparseFieldSet(),
                TestExpressionFactory.Instance.SparseFieldSet()
            ],
            [
                TestExpressionFactory.Instance.SparseFieldSetElement(),
                TestExpressionFactory.Instance.SparseFieldSetElement()
            ]
        };

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Expressions_are_equal(QueryExpression left, QueryExpression right)
    {
        // Assert
        left.Equals(right).Should().BeTrue();
        right.Equals(left).Should().BeTrue();

        // ReSharper disable once EqualExpressionComparison
        left.Equals(left).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Expressions_are_not_equal_to_null(QueryExpression left, QueryExpression right)
    {
        // Assert
        left.Equals(null).Should().BeFalse();
        right.Equals(null).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Expressions_have_same_hash_code(QueryExpression left, QueryExpression right)
    {
        // Assert
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Expressions_convert_to_same_string(QueryExpression left, QueryExpression right)
    {
        // Assert
        left.ToString().Should().Be(right.ToString());
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Expressions_convert_to_same_full_string(QueryExpression left, QueryExpression right)
    {
        // Assert
        left.ToFullString().Should().Be(right.ToFullString());
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Function_expressions_have_same_return_type(QueryExpression left, QueryExpression right)
    {
        if (left is FunctionExpression leftFunction && right is FunctionExpression rightFunction)
        {
            // Assert
            leftFunction.ReturnType.Should().Be(rightFunction.ReturnType);
        }
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Scalar_collection_expressions_have_same_return_type(QueryExpression left, QueryExpression right)
    {
        if (left is ScalarCollectionElementValueExpression leftElement && right is ScalarCollectionElementValueExpression rightElement)
        {
            // Assert
            leftElement.ElementType.Should().Be(rightElement.ElementType);
        }
    }

    [Theory]
    [MemberData(nameof(ExpressionTestData))]
    public void Expressions_can_accept_visitor(QueryExpression left, QueryExpression right)
    {
        // Assert
        left.Accept(EmptyQueryExpressionVisitor.Instance, null).Should().BeNull();
        right.Accept(EmptyQueryExpressionVisitor.Instance, null).Should().BeNull();
    }

    [Fact]
    public void ScalarCollectionElementValue_ToString_Equals_GetHashCode()
    {
        // Arrange
        var stringElement = new ScalarCollectionElementValueExpression(typeof(string));
        var otherStringElement = new ScalarCollectionElementValueExpression(typeof(string));
        var guidElement = new ScalarCollectionElementValueExpression(typeof(Guid));

        // Act and assert
        stringElement.ToString().Should().Be("_");
        stringElement.ToFullString().Should().Be("_");
        guidElement.ToString().Should().Be("_");

        stringElement.Should().Be(otherStringElement);
        stringElement.GetHashCode().Should().Be(otherStringElement.GetHashCode());
        stringElement.Should().NotBe(guidElement);
        stringElement.GetHashCode().Should().NotBe(guidElement.GetHashCode());
    }

    [Fact]
    public void SparseFieldSetElement_equality_includes_children()
    {
        // Arrange
        CompoundFixture fixture = CompoundFixture.Instance;

        SparseFieldSetElementExpression streetAndCity = fixture.CreateAddressElement(fixture.StreetAttribute, fixture.CityAttribute);
        SparseFieldSetElementExpression cityAndStreet = fixture.CreateAddressElement(fixture.CityAttribute, fixture.StreetAttribute);
        SparseFieldSetElementExpression cityOnly = fixture.CreateAddressElement(fixture.CityAttribute);
        SparseFieldSetElementExpression whole = fixture.CreateAddressElement();

        // Act and assert
        streetAndCity.Should().Be(cityAndStreet);
        streetAndCity.GetHashCode().Should().Be(cityAndStreet.GetHashCode());
        streetAndCity.ToString().Should().Be(cityAndStreet.ToString());

        streetAndCity.Should().NotBe(cityOnly);
        cityOnly.Should().NotBe(whole);
        streetAndCity.Should().NotBe(whole);
        whole.ToString().Should().Be("address");
        streetAndCity.ToString().Should().Be("address.city,address.street");
    }

    [Fact]
    public void SparseFieldSet_Create_and_Empty()
    {
        // Arrange
        CompoundFixture fixture = CompoundFixture.Instance;

        // Act
        SparseFieldSetExpression emptySet = SparseFieldSetExpression.Create(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty);

        SparseFieldSetExpression nonEmptySet = SparseFieldSetExpression.Create(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty
            .Add(fixture.AddressAttribute, fixture.CreateAddressElement()));

        // Assert
        emptySet.Should().BeSameAs(SparseFieldSetExpression.Empty);
        SparseFieldSetExpression.Empty.Elements.Should().BeEmpty();
        SparseFieldSetExpression.Empty.ToString().Should().BeEmpty();

        nonEmptySet.Should().NotBeSameAs(SparseFieldSetExpression.Empty);
        nonEmptySet.Elements.Should().HaveCount(1);
    }

    [Fact]
    public void Has_without_filter_renders_as_has_collection()
    {
        // Arrange
        var has = new HasExpression(TestExpressionFactory.Instance.ResourceFieldChainForChildren(), null);

        // Act and assert
        has.ToString().Should().Be("has(children)");
        has.ToFullString().Should().Be("has(baseTestResources:children)");
    }

    [Fact]
    public void Can_convert_QueryLayer_to_string()
    {
        // Arrange
        QueryLayer queryLayer = TestQueryLayerFactory.Instance.Default();

        // Act
        string text = queryLayer.ToString();

        // Assert
        text.Should().Be("""
            QueryLayer<DerivedTestResource>
            {
              Include: parent.children
              Filter: and(contains(text,'example'),not(equals(text,'example')))
              Sort: -count(children)
              Pagination: Page number: 2, size: 5
              Selection
              {
                FieldSelectors<DerivedTestResource>
                {
                  text
                  parent: QueryLayer<DerivedTestResource>
                  {
                    Selection
                    {
                      FieldSelectors<DerivedTestResource>
                      {
                        text
                      }
                    }
                  }
                  children: QueryLayer<DerivedTestResource>
                  {
                    Selection
                    {
                      FieldSelectors<DerivedTestResource>
                      {
                        text
                      }
                    }
                  }
                }
              }
            }

            """);
    }

    [Fact]
    public void Can_convert_QueryLayer_to_full_string()
    {
        // Arrange
        QueryLayer queryLayer = TestQueryLayerFactory.Instance.Default();

        // Act
        string text = queryLayer.ToFullString();

        // Assert
        text.Should().Be("""
            QueryLayer<DerivedTestResource>
            {
              Include: baseTestResources:parent.baseTestResources:children
              Filter: and(contains(baseTestResources:text,'example'),not(equals(baseTestResources:text,'example')))
              Sort: -count(baseTestResources:children)
              Pagination: Page number: 2, size: 5
              Selection
              {
                FieldSelectors<DerivedTestResource>
                {
                  derivedTestResources:text
                  derivedTestResources:parent: QueryLayer<DerivedTestResource>
                  {
                    Selection
                    {
                      FieldSelectors<DerivedTestResource>
                      {
                        derivedTestResources:text
                      }
                    }
                  }
                  derivedTestResources:children: QueryLayer<DerivedTestResource>
                  {
                    Selection
                    {
                      FieldSelectors<DerivedTestResource>
                      {
                        derivedTestResources:text
                      }
                    }
                  }
                }
              }
            }

            """);
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private class BaseTestResource : Identifiable<Guid>
    {
        [Attr]
        public string? Text { get; set; }

        [HasOne]
        public BaseTestResource? Parent { get; set; }

        [HasMany]
        public ISet<BaseTestResource> Children { get; set; } = new HashSet<BaseTestResource>();
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class DerivedTestResource : BaseTestResource;

    private sealed class TestExpressionFactory
    {
        private readonly ResourceType _derivedTestResourceType;
        private readonly AttrAttribute _textAttribute;
        private readonly RelationshipAttribute _parentRelationship;
        private readonly RelationshipAttribute _childrenRelationship;

        public static TestExpressionFactory Instance { get; } = new();

        private TestExpressionFactory()
        {
            var options = new JsonApiOptions();

            var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            builder.Add<BaseTestResource, Guid>();
            builder.Add<DerivedTestResource, Guid>();
            IResourceGraph resourceGraph = builder.Build();

            ResourceType baseTestResourceType = resourceGraph.GetResourceType<BaseTestResource>();
            _derivedTestResourceType = resourceGraph.GetResourceType<DerivedTestResource>();
            _textAttribute = baseTestResourceType.GetAttributeByPropertyName(nameof(BaseTestResource.Text));
            _parentRelationship = baseTestResourceType.GetRelationshipByPropertyName(nameof(BaseTestResource.Parent));
            _childrenRelationship = baseTestResourceType.GetRelationshipByPropertyName(nameof(BaseTestResource.Children));
        }

        public AnyExpression Any()
        {
            return new AnyExpression(ResourceFieldChainForText(), [LiteralConstant()]);
        }

        public ComparisonExpression Comparison()
        {
            return new ComparisonExpression(ComparisonOperator.Equals, ResourceFieldChainForText(), LiteralConstant());
        }

        public CountExpression Count()
        {
            return new CountExpression(ResourceFieldChainForChildren());
        }

        public ScalarCollectionElementValueExpression ScalarCollectionElementValue()
        {
            return new ScalarCollectionElementValueExpression(typeof(string));
        }

        public HasExpression Has()
        {
            return new HasExpression(ResourceFieldChainForChildren(), Comparison());
        }

        public IncludeElementExpression IncludeElement()
        {
            return new IncludeElementExpression(_parentRelationship, [new IncludeElementExpression(_childrenRelationship)]);
        }

        public IncludeExpression Include()
        {
            return new IncludeExpression([IncludeElement()]);
        }

        public IsTypeExpression IsType()
        {
            return new IsTypeExpression(ResourceFieldChainForParent(), _derivedTestResourceType, Has());
        }

        public LiteralConstantExpression LiteralConstant()
        {
            return new LiteralConstantExpression("example");
        }

        public LogicalExpression Logical()
        {
            return new LogicalExpression(LogicalOperator.And, MatchText(), Not());
        }

        public MatchTextExpression MatchText()
        {
            return new MatchTextExpression(ResourceFieldChainForText(), LiteralConstant(), TextMatchKind.Contains);
        }

        public NotExpression Not()
        {
            return new NotExpression(Comparison());
        }

        public NullConstantExpression NullConstant()
        {
            return NullConstantExpression.Instance;
        }

        public PaginationElementQueryStringValueExpression PaginationElementQueryStringValue()
        {
            return new PaginationElementQueryStringValueExpression(Include(), 5, 8);
        }

        public PaginationExpression Pagination()
        {
            return new PaginationExpression(new PageNumber(2), new PageSize(5));
        }

        public PaginationQueryStringValueExpression PaginationQueryStringValue()
        {
            return new PaginationQueryStringValueExpression([PaginationElementQueryStringValue()]);
        }

        public QueryableHandlerExpression QueryableHandler()
        {
#pragma warning disable CS8974 // Converting method group to non-delegate type
            object handler = TestQueryableHandler;
#pragma warning restore CS8974 // Converting method group to non-delegate type
            return new QueryableHandlerExpression(handler, "disableCache");
        }

        public QueryStringParameterScopeExpression QueryStringParameterScope()
        {
            return new QueryStringParameterScopeExpression(LiteralConstant(), Include());
        }

        public ResourceFieldChainExpression ResourceFieldChainForText()
        {
            return new ResourceFieldChainExpression(_textAttribute);
        }

        public ResourceFieldChainExpression ResourceFieldChainForParent()
        {
            return new ResourceFieldChainExpression([_parentRelationship]);
        }

        public ResourceFieldChainExpression ResourceFieldChainForChildren()
        {
            return new ResourceFieldChainExpression([_childrenRelationship]);
        }

        public SortElementExpression SortElement()
        {
            return new SortElementExpression(Count(), false);
        }

        public SortExpression Sort()
        {
            return new SortExpression([SortElement()]);
        }

        public SparseFieldSetExpression SparseFieldSet()
        {
            return new SparseFieldSetExpression(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty
                .Add(_textAttribute, new SparseFieldSetElementExpression(_textAttribute))
                .Add(_childrenRelationship, new SparseFieldSetElementExpression(_childrenRelationship)));
        }

        public SparseFieldSetElementExpression SparseFieldSetElement()
        {
            return new SparseFieldSetElementExpression(_textAttribute);
        }

        private static IQueryable<BaseTestResource> TestQueryableHandler(IQueryable<BaseTestResource> source, StringValues parameterValue)
        {
            throw new NotImplementedException();
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundResource : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundAddress? Address { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundAddress
    {
        [Attr]
        public string? Street { get; set; }

        [Attr]
        public string? City { get; set; }
    }

    private sealed class CompoundFixture
    {
        public static CompoundFixture Instance { get; } = new();

        public AttrAttribute AddressAttribute { get; }
        public AttrAttribute StreetAttribute { get; }
        public AttrAttribute CityAttribute { get; }

        private CompoundFixture()
        {
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<CompoundResource, long>().Build();
            ResourceType resourceType = resourceGraph.GetResourceType<CompoundResource>();

            AddressAttribute = resourceType.GetAttributeByPropertyName(nameof(CompoundResource.Address));
            StreetAttribute = AddressAttribute.CompoundType!.GetAttributeByPropertyName(nameof(CompoundAddress.Street));
            CityAttribute = AddressAttribute.CompoundType.GetAttributeByPropertyName(nameof(CompoundAddress.City));
        }

        public SparseFieldSetElementExpression CreateAddressElement(params AttrAttribute[] children)
        {
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> childElements =
                children.ToImmutableDictionary(child => (ResourceFieldAttribute)child, child => new SparseFieldSetElementExpression(child));

            return new SparseFieldSetElementExpression(AddressAttribute, childElements);
        }
    }

    private sealed class EmptyQueryExpressionVisitor : QueryExpressionVisitor<BaseTestResource?, object?>
    {
        public static EmptyQueryExpressionVisitor Instance { get; } = new();

        private EmptyQueryExpressionVisitor()
        {
        }
    }

    private sealed class TestQueryLayerFactory
    {
        public static TestQueryLayerFactory Instance { get; } = new();

        private TestQueryLayerFactory()
        {
        }

        public QueryLayer Default()
        {
            var options = new JsonApiOptions();

            var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            builder.Add<BaseTestResource, Guid>();
            builder.Add<DerivedTestResource, Guid>();
            IResourceGraph resourceGraph = builder.Build();

            ResourceType resourceType = resourceGraph.GetResourceType<DerivedTestResource>();
            AttrAttribute textAttribute = resourceType.GetAttributeByPropertyName(nameof(DerivedTestResource.Text));
            RelationshipAttribute parentRelationship = resourceType.GetRelationshipByPropertyName(nameof(DerivedTestResource.Parent));
            RelationshipAttribute childrenRelationship = resourceType.GetRelationshipByPropertyName(nameof(DerivedTestResource.Children));

            return new QueryLayer(resourceType)
            {
                Include = TestExpressionFactory.Instance.Include(),
                Filter = TestExpressionFactory.Instance.Logical(),
                Sort = TestExpressionFactory.Instance.Sort(),
                Pagination = TestExpressionFactory.Instance.Pagination(),
                Selection = new FieldSelection
                {
                    [resourceType] = new FieldSelectors
                    {
                        [textAttribute] = null,
                        [parentRelationship] = new QueryLayer(resourceType)
                        {
                            Selection = new FieldSelection
                            {
                                [resourceType] = new FieldSelectors
                                {
                                    [textAttribute] = null
                                }
                            }
                        },
                        [childrenRelationship] = new QueryLayer(resourceType)
                        {
                            Selection = new FieldSelection
                            {
                                [resourceType] = new FieldSelectors
                                {
                                    [textAttribute] = null
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
