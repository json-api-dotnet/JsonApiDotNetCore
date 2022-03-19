using System.ComponentModel;
using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceDefinitions;

public sealed class CreateSortExpressionFromLambdaTests
{
    [Fact]
    public void Can_convert_chain_of_ToOne_relationships_ending_in_attribute()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        SortExpression expression = resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => file.Content, ListSortDirection.Descending),
            (file => file.Name, ListSortDirection.Ascending),
            (file => file.Length, ListSortDirection.Ascending),
            (file => file.Parent.Name, ListSortDirection.Ascending),
            (file => file.Parent.Parent.Name, ListSortDirection.Ascending)
        });

        // Assert
        string[] expected =
        {
            "-fileEntries:content",
            "fileSystemEntries:name",
            "fileEntries:length",
            "fileSystemEntries:parent.fileSystemEntries:name",
            "fileSystemEntries:parent.fileSystemEntries:parent.fileSystemEntries:name"
        };

        expression.ToFullString().Should().Be(string.Join(',', expected));
    }

    [Fact]
    public void Can_convert_chain_of_ToOne_relationships_ending_in_count_of_ToMany_relationship()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<DirectoryEntry, long>(resourceGraph);

        // Act
        SortExpression expression = resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<DirectoryEntry, long>.PropertySortOrder
        {
            (directory => directory.Subdirectories.Count, ListSortDirection.Ascending),
            // ReSharper disable once UseCollectionCountProperty
            (directory => directory.Files.Count(), ListSortDirection.Descending),
            (directory => directory.Children.Count, ListSortDirection.Ascending),
            (directory => directory.Parent.Children.Count, ListSortDirection.Ascending),
            (directory => directory.Parent.Parent.Children.Count, ListSortDirection.Ascending)
        });

        // Assert
        string[] expected =
        {
            "count(directoryEntries:subdirectories)",
            "-count(directoryEntries:files)",
            "count(fileSystemEntries:children)",
            "count(fileSystemEntries:parent.fileSystemEntries:children)",
            "count(fileSystemEntries:parent.fileSystemEntries:parent.fileSystemEntries:children)"
        };

        expression.ToFullString().Should().Be(string.Join(',', expected));
    }

    [Fact]
    public void Can_convert_chain_with_conversion_to_derived_types()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileSystemEntry, long>(resourceGraph);

        // Act
        SortExpression expression = resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileSystemEntry, long>.PropertySortOrder
        {
            (entry => ((FileEntry)entry).Content, ListSortDirection.Ascending),
            (entry => (entry.Parent as FileEntry)!.Content, ListSortDirection.Ascending),
            (entry => ((DirectoryEntry)entry).Subdirectories.Count, ListSortDirection.Ascending),
            (entry => ((DirectoryEntry)((FileEntry)entry).Parent).Files.Count, ListSortDirection.Ascending),
            (entry => ((DirectoryEntry)(FileSystemEntry)(FileEntry)entry).Name, ListSortDirection.Descending)
        });

        // Assert
        string[] expected =
        {
            "fileEntries:content",
            "fileSystemEntries:parent.fileEntries:content",
            "count(directoryEntries:subdirectories)",
            "count(fileSystemEntries:parent.directoryEntries:files)",
            "-fileSystemEntries:name"
        };

        expression.ToFullString().Should().Be(string.Join(',', expected));
    }

    [Fact]
    public void Cannot_convert_unexposed_attribute()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => file.IsCompressed, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().StartWith("The lambda expression 'file => Convert(file.IsCompressed, Object)' is invalid. ");
        exception.Errors[0].Detail.Should().EndWith("Expected property for JSON:API attribute, but found 'file.IsCompressed'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_unexposed_ToMany_relationship()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => file.Content.Length, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().StartWith("The lambda expression 'file => Convert(file.Content.Length, Object)' is invalid. ");
        exception.Errors[0].Detail.Should().EndWith("Expected property for JSON:API to-many relationship, but found 'file.Content'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_unexposed_ToOne_relationship()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => file.ParentDirectory!.Name, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().StartWith("The lambda expression 'file => file.ParentDirectory.Name' is invalid. ");
        exception.Errors[0].Detail.Should().EndWith("Expected property for JSON:API to-one relationship, but found 'file.ParentDirectory'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_unexposed_resource_type()
    {
        // Arrange
        IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<FileSystemEntry, long>().Build();

        var resourceDefinition = new WrapperResourceDefinition<FileSystemEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileSystemEntry, long>.PropertySortOrder
        {
            (entry => ((FileEntry)entry).Content, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().StartWith("The lambda expression 'entry => Convert(entry, FileEntry).Content' is invalid. ");
        exception.Errors[0].Detail.Should().EndWith("Resource of type 'FileEntry' does not exist.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_count_with_predicate()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<DirectoryEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<DirectoryEntry, long>.PropertySortOrder
        {
            (directory => directory.Files.Count(_ => true), ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().StartWith("The lambda expression 'directory => Convert(directory.Files.Count(_ => True), Object)' is invalid. ");
        exception.Errors[0].Detail.Should().EndWith("Count method that takes a predicate is not supported.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_null_selector()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileSystemEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileSystemEntry, long>.PropertySortOrder
        {
            (_ => null, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().Be("The lambda expression '_ => null' is invalid. Unsupported expression body 'null'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_self_selector()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileSystemEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileSystemEntry, long>.PropertySortOrder
        {
            (entry => entry, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().Be("The lambda expression 'entry => entry' is invalid. Unsupported expression body 'entry'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_conditional_operator()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => file.IsCompressed ? file.Content : file.Length, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().Match("The lambda expression '*' is invalid. Unsupported expression body '*'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_concatenation_operator()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => file.Name + ":" + file.Content, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().Match("The lambda expression '*' is invalid. Unsupported expression body '*'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    [Fact]
    public void Cannot_convert_projection_into_anonymous_type()
    {
        // Arrange
        IResourceGraph resourceGraph = GetResourceGraph();

        var resourceDefinition = new WrapperResourceDefinition<FileEntry, long>(resourceGraph);

        // Act
        Action action = () => resourceDefinition.GetSortExpressionFromLambda(new JsonApiResourceDefinition<FileEntry, long>.PropertySortOrder
        {
            (file => new
            {
                file.Length,
                file.Content
            }, ListSortDirection.Ascending)
        });

        // Assert
        JsonApiException exception = action.Should().ThrowExactly<JsonApiException>().Which;

        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Errors[0].Title.Should().StartWith("Invalid lambda expression for sorting from resource definition. It should ");
        exception.Errors[0].Detail.Should().Match("The lambda expression '*' is invalid. Unsupported expression body '*'.");
        exception.Errors[0].Source.Should().BeNull();
    }

    private static IResourceGraph GetResourceGraph()
    {
        var options = new JsonApiOptions();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        return new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
            .Add<FileSystemEntry, long>()
            .Add<DirectoryEntry, long>()
            .Add<FileEntry, long>()
            .Build();

        // @formatter:wrap_chained_method_calls restore
        // @formatter:keep_existing_linebreaks restore
    }

    private sealed class WrapperResourceDefinition<TResource, TId> : JsonApiResourceDefinition<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        public WrapperResourceDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

        public SortExpression GetSortExpressionFromLambda(PropertySortOrder sortOrder)
        {
            return CreateSortExpressionFromLambda(sortOrder);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private abstract class FileSystemEntry : Identifiable<long>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasOne]
        public FileSystemEntry Parent { get; set; } = null!;

        [HasMany]
        public IList<FileSystemEntry> Children { get; set; } = new List<FileSystemEntry>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DirectoryEntry : FileSystemEntry
    {
        [HasMany]
        public IList<DirectoryEntry> Subdirectories { get; set; } = new List<DirectoryEntry>();

        [HasMany]
        public IList<FileEntry> Files { get; set; } = new List<FileEntry>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class FileEntry : FileSystemEntry
    {
        [Attr]
        public string Content { get; set; } = null!;

        [Attr]
        public ulong Length { get; set; }

        public bool IsCompressed { get; set; }

        public DirectoryEntry? ParentDirectory { get; set; }
    }
}
