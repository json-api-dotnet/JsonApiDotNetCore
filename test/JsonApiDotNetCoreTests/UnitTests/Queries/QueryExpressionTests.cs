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
                TestExpressionFactory.Instance.SparseFieldTable(),
                TestExpressionFactory.Instance.SparseFieldTable()
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
    public void Expressions_have_same_return_type(QueryExpression left, QueryExpression right)
    {
        if (left is FunctionExpression leftFunction && right is FunctionExpression rightFunction)
        {
            // Assert
            leftFunction.ReturnType.Should().Be(rightFunction.ReturnType);
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
        private readonly ResourceType _baseTestResourceType;
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

            _baseTestResourceType = resourceGraph.GetResourceType<BaseTestResource>();
            _derivedTestResourceType = resourceGraph.GetResourceType<DerivedTestResource>();
            _textAttribute = _baseTestResourceType.GetAttributeByPropertyName(nameof(BaseTestResource.Text));
            _parentRelationship = _baseTestResourceType.GetRelationshipByPropertyName(nameof(BaseTestResource.Parent));
            _childrenRelationship = _baseTestResourceType.GetRelationshipByPropertyName(nameof(BaseTestResource.Children));
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
            return new SparseFieldSetExpression([
                _textAttribute,
                _childrenRelationship
            ]);
        }

        public SparseFieldTableExpression SparseFieldTable()
        {
            return new SparseFieldTableExpression(new Dictionary<ResourceType, SparseFieldSetExpression>
            {
                [_baseTestResourceType] = SparseFieldSet(),
                [_derivedTestResourceType] = SparseFieldSet()
            }.ToImmutableDictionary());
        }

        private static IQueryable<BaseTestResource> TestQueryableHandler(IQueryable<BaseTestResource> source, StringValues parameterValue)
        {
            throw new NotImplementedException();
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
