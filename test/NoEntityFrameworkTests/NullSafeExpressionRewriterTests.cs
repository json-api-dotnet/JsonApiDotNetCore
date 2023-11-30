using System.Linq.Expressions;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using NoEntityFrameworkExample;
using Xunit;

namespace NoEntityFrameworkTests;

public sealed class NullSafeExpressionRewriterTests
{
    [Fact]
    public void Can_rewrite_where_clause_with_constant_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            }
        };

        TestResource lastInDataSource = dataSource.Last();

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.Where(resource => resource.Parent!.Id == 3);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.Where(resource => ((resource.Parent != null) AndAlso (resource.Parent.Id == 3)))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(1);
        resources[0].Id.Should().Be(lastInDataSource.Id);
    }

    [Fact]
    public void Can_rewrite_where_clause_with_member_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                },
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext()
                    }
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                },
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext(),
                        Parent = new TestResource()
                    }
                }
            }
        };

        TestResource lastInDataSource = dataSource.Last();
        lastInDataSource.FirstChild!.Parent!.Id = lastInDataSource.Parent!.Id;

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source =>
            source.Where(resource => resource.Parent!.Id == resource.FirstChild!.Parent!.Id);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be(
            "source => source.Where(resource => ((resource.Parent != null) AndAlso ((resource.FirstChild != null) AndAlso ((resource.FirstChild.Parent != null) AndAlso (resource.Parent.Id == resource.FirstChild.Parent.Id)))))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(1);
        resources[0].Id.Should().Be(lastInDataSource.Id);
    }

    [Fact]
    public void Can_rewrite_where_clause_with_not_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            }
        };

        // ReSharper disable once NegativeEqualityExpression
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.Where(resource => !(resource.Parent!.Id == 3));

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.Where(resource => Not(((resource.Parent != null) AndAlso (resource.Parent.Id == 3))))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(1);
        resources[0].Id.Should().Be(dataSource[0].Id);
    }

    [Fact]
    public void Can_rewrite_where_clause_with_any_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Parent = new TestResource
                    {
                        Id = generator.GetNext(),
                        Children =
                        {
                            new TestResource
                            {
                                Id = generator.GetNext()
                            }
                        }
                    }
                }
            }
        };

        TestResource lastInDataSource = dataSource.Last();

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source =>
            source.Where(resource => resource.Parent!.Parent!.Children.Any());

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be(
            "source => source.Where(resource => ((resource.Parent != null) AndAlso ((resource.Parent.Parent != null) AndAlso ((resource.Parent.Parent.Children != null) AndAlso resource.Parent.Parent.Children.Any()))))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(1);
        resources[0].Id.Should().Be(lastInDataSource.Id);
    }

    [Fact]
    public void Can_rewrite_where_clause_with_conditional_any_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext(),
                        Children =
                        {
                            new TestResource
                            {
                                Id = generator.GetNext()
                            }
                        }
                    }
                }
            }
        };

        // ReSharper disable once NegativeEqualityExpression
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.Where(resource =>
            resource.Parent!.Id == 3 || resource.FirstChild!.Children.Any(child => !(child.Parent!.Id == 1)));

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.Where(resource => (((resource.Parent != null) AndAlso (resource.Parent.Id == 3)) OrElse " +
            "((resource.FirstChild != null) AndAlso ((resource.FirstChild.Children != null) AndAlso resource.FirstChild.Children.Any(child => Not(((child.Parent != null) AndAlso (child.Parent.Id == 1))))))))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(2);
        resources[0].Id.Should().Be(dataSource[1].Id);
        resources[1].Id.Should().Be(dataSource[2].Id);
    }

    [Fact]
    public void Can_rewrite_where_clause_with_nested_conditional_any_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext(),
                Children = null!
            },
            new()
            {
                Id = generator.GetNext(),
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext(),
                        Children =
                        {
                            new TestResource
                            {
                                Id = generator.GetNext()
                            }
                        }
                    }
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext(),
                        Children =
                        {
                            new TestResource
                            {
                                Id = generator.GetNext(),
                                Parent = new TestResource
                                {
                                    Id = generator.GetNext(),
                                    Name = "Jack"
                                }
                            }
                        }
                    }
                }
            }
        };

        TestResource lastInDataSource = dataSource.Last();

        // ReSharper disable once NegativeEqualityExpression
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.Where(resource =>
            resource.Children.Any(child => child.Children.Any(childOfChild => childOfChild.Parent!.Name == "Jack")));

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.Where(resource => " +
            "((resource.Children != null) AndAlso resource.Children.Any(child => ((child.Children != null) AndAlso child.Children.Any(childOfChild => ((childOfChild.Parent != null) AndAlso (childOfChild.Parent.Name == \"Jack\")))))))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(1);
        resources[0].Id.Should().Be(lastInDataSource.Id);
    }

    [Fact]
    public void Can_rewrite_where_clause_with_count_comparison()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                },
                Children = null!
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Children =
                    {
                        new TestResource
                        {
                            Id = generator.GetNext()
                        }
                    }
                },
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext()
                    },
                    new TestResource
                    {
                        Id = generator.GetNext()
                    }
                }
            }
        };

        TestResource lastInDataSource = dataSource.Last();

        // ReSharper disable UseCollectionCountProperty
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source =>
            source.Where(resource => resource.Children.Count() > resource.Parent!.Children.Count());
        // ReSharper restore UseCollectionCountProperty

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be(
            "source => source.Where(resource => ((resource.Children != null) AndAlso ((resource.Parent != null) AndAlso ((resource.Parent.Children != null) AndAlso (resource.Children.Count() > resource.Parent.Children.Count())))))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(1);
        resources[0].Id.Should().Be(lastInDataSource.Id);
    }

    [Fact]
    public void Can_rewrite_order_by_clause_with_long()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            }
        };

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.OrderBy(resource => resource.Parent!.Id);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.OrderBy(resource => IIF((resource.Parent == null), -9223372036854775808, resource.Parent.Id))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(3);
        resources[0].Id.Should().Be(dataSource[0].Id);
        resources[1].Id.Should().Be(dataSource[1].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
    }

    [Fact]
    public void Can_rewrite_order_by_clause_with_IntPtr()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
#if NET6_0
                    Pointer = (IntPtr)1
#else
                    Pointer = 1
#endif
                }
            }
        };

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.OrderBy(resource => resource.Parent!.Pointer);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should()
            .Be("source => source.OrderBy(resource => IIF((resource.Parent == null), -9223372036854775808, resource.Parent.Pointer))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(3);
        resources[0].Id.Should().Be(dataSource[0].Id);
        resources[1].Id.Should().Be(dataSource[1].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
    }

    [Fact]
    public void Can_rewrite_order_by_clause_with_nullable_int()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Number = -1
                }
            }
        };

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.OrderBy(resource => resource.Parent!.Number);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.OrderBy(resource => IIF((resource.Parent == null), null, resource.Parent.Number))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(3);
        resources[0].Id.Should().Be(dataSource[0].Id);
        resources[1].Id.Should().Be(dataSource[1].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
    }

    [Fact]
    public void Can_rewrite_order_by_clause_with_enum()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Enum = TestEnum.Two
                }
            }
        };

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.OrderBy(resource => resource.Parent!.Enum);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.OrderBy(resource => IIF((resource.Parent == null), Zero, resource.Parent.Enum))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(3);
        resources[0].Id.Should().Be(dataSource[0].Id);
        resources[1].Id.Should().Be(dataSource[1].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
    }

    [Fact]
    public void Can_rewrite_order_by_clause_with_string()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Name = "X"
                }
            }
        };

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.OrderBy(resource => resource.Parent!.Name);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be("source => source.OrderBy(resource => IIF((resource.Parent == null), null, resource.Parent.Name))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(3);
        resources[0].Id.Should().Be(dataSource[0].Id);
        resources[1].Id.Should().Be(dataSource[1].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
    }

    [Fact]
    public void Can_rewrite_order_by_clause_with_count()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                },
                Children = null!
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext()
                },
                Children =
                {
                    new TestResource
                    {
                        Id = generator.GetNext()
                    }
                }
            }
        };

        // ReSharper disable once UseCollectionCountProperty
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source =>
            source.OrderBy(resource => resource.Parent!.Children.Count());

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be(
            "source => source.OrderBy(resource => IIF((resource.Parent == null), -2147483648, IIF((resource.Parent.Children == null), -2147483648, resource.Parent.Children.Count())))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(4);
        resources[0].Id.Should().Be(dataSource[0].Id);
        resources[1].Id.Should().Be(dataSource[1].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
        resources[3].Id.Should().Be(dataSource[3].Id);
    }

    [Fact]
    public void Can_rewrite_nested_descending_order_by_clauses()
    {
        // Arrange
        var generator = new IdGenerator();

        var dataSource = new List<TestResource>
        {
            new()
            {
                Id = generator.GetNext()
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Name = "A",
                    Number = 1
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Name = "A",
                    Number = 10
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Name = "Z",
                    Number = 1
                }
            },
            new()
            {
                Id = generator.GetNext(),
                Parent = new TestResource
                {
                    Id = generator.GetNext(),
                    Name = "Z",
                    Number = 10
                }
            }
        };

        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source =>
            source.OrderByDescending(resource => resource.Parent!.Name).ThenByDescending(resource => resource.Parent!.Number);

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be(
            "source => source.OrderByDescending(resource => IIF((resource.Parent == null), null, resource.Parent.Name)).ThenByDescending(resource => IIF((resource.Parent == null), null, resource.Parent.Number))");

        TestResource[] resources = DynamicInvoke(safeExpression, dataSource);
        resources.Should().HaveCount(5);
        resources[0].Id.Should().Be(dataSource[4].Id);
        resources[1].Id.Should().Be(dataSource[3].Id);
        resources[2].Id.Should().Be(dataSource[2].Id);
        resources[3].Id.Should().Be(dataSource[1].Id);
        resources[4].Id.Should().Be(dataSource[0].Id);
    }

    [Fact]
    public void Does_not_rewrite_in_select()
    {
        // Arrange
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression = source => source.Select(resource => new TestResource
        {
            Id = resource.Id,
            Name = resource.Name,
            Parent = resource.Parent,
            Children = resource.Children
        });

        var rewriter = new NullSafeExpressionRewriter();

        // Act
        Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> safeExpression = rewriter.Rewrite(expression);

        // Assert
        safeExpression.ToString().Should().Be(expression.ToString());
    }

    private static TestResource[] DynamicInvoke(Expression<Func<IEnumerable<TestResource>, IEnumerable<TestResource>>> expression,
        IEnumerable<TestResource> dataSource)
    {
        Delegate function = expression.Compile();
        object enumerable = function.DynamicInvoke(dataSource)!;
        return ((IEnumerable<TestResource>)enumerable).ToArray();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class TestResource : Identifiable<long>
    {
        [Attr]
        public string? Name { get; set; }

        [Attr]
        public int? Number { get; set; }

        [Attr]
        public IntPtr Pointer { get; set; }

        [Attr]
        public TestEnum Enum { get; set; }

        [HasOne]
        public TestResource? Parent { get; set; }

        [HasOne]
        public TestResource? FirstChild => Children.FirstOrDefault();

        [HasMany]
        public ISet<TestResource> Children { get; set; } = new HashSet<TestResource>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private enum TestEnum
    {
        Zero,
        One,
        Two
    }

    private sealed class IdGenerator
    {
        private long _lastId;

        public long GetNext()
        {
            return ++_lastId;
        }
    }
}
