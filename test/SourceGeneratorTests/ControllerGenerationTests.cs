using FluentAssertions;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceGeneratorTests;

public sealed class ControllerGenerationTests
{
    [Fact]
    public void Can_generate_for_default_controller()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using ExampleApi.Models;

namespace ExampleApi.Controllers
{
    public sealed partial class ItemsController : JsonApiController<Item, long>
    {
        public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Item, long> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
");
    }

    [Fact]
    public void Can_generate_for_read_only_controller()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .WithNamespaceImportFor(typeof(JsonApiEndpoints))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query)]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using ExampleApi.Models;

namespace ExampleApi.Controllers
{
    public sealed partial class ItemsController : JsonApiQueryController<Item, long>
    {
        public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceQueryService<Item, long> queryService)
            : base(options, resourceGraph, loggerFactory, queryService)
        {
        }
    }
}
");
    }

    [Fact]
    public void Can_generate_for_write_only_controller()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .WithNamespaceImportFor(typeof(JsonApiEndpoints))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.Command)]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using ExampleApi.Models;

namespace ExampleApi.Controllers
{
    public sealed partial class ItemsController : JsonApiCommandController<Item, long>
    {
        public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceCommandService<Item, long> commandService)
            : base(options, resourceGraph, loggerFactory, commandService)
        {
        }
    }
}
");
    }

    [Fact]
    public void Can_generate_for_mixed_controller()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .WithNamespaceImportFor(typeof(JsonApiEndpoints))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource(GenerateControllerEndpoints = NoRelationshipEndpoints)]
                    public sealed class Item : Identifiable<long>
                    {
                        private const JsonApiEndpoints NoRelationshipEndpoints = JsonApiEndpoints.GetCollection |
                            JsonApiEndpoints.GetSingle | JsonApiEndpoints.Post | JsonApiEndpoints.Patch | JsonApiEndpoints.Delete;

                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using ExampleApi.Models;

namespace ExampleApi.Controllers
{
    public sealed partial class ItemsController : JsonApiController<Item, long>
    {
        public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IGetAllService<Item, long> getAll,
            IGetByIdService<Item, long> getById,
            ICreateService<Item, long> create,
            IUpdateService<Item, long> update,
            IDeleteService<Item, long> delete)
            : base(options, resourceGraph, loggerFactory,
                getAll: getAll,
                getById: getById,
                create: create,
                update: update,
                delete: delete)
        {
        }
    }
}
");
    }

    [Fact]
    public void Skips_for_resource_without_ResourceAttribute()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(AttrAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();
        runResult.Should().NotHaveProducedSourceCode();
    }

    [Fact]
    public void Skips_for_resource_with_no_endpoints()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .WithNamespaceImportFor(typeof(JsonApiEndpoints))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();
        runResult.Should().NotHaveProducedSourceCode();
    }

    [Fact]
    public void Skips_for_missing_dependency_on_JsonApiDotNetCore()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    public abstract class Identifiable<TId>
                    {
                    }

                    public sealed class ResourceAttribute : System.Attribute
                    {
                    }

                    public sealed class AttrAttribute : System.Attribute
                    {
                    }

                    [Resource]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithSystemReferences()
            .WithLoggerFactoryReference()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().NotHaveProducedSourceCode();
    }

    [Fact]
    public void Skips_for_missing_dependency_on_LoggerFactory()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithSystemReferences()
            .WithJsonApiDotNetCoreReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().NotHaveProducedSourceCode();
    }

    [Fact]
    public void Warns_for_resource_that_does_not_implement_IIdentifiable()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource]
                    public sealed class Item
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();

        runResult.Should()
            .HaveSingleDiagnostic(
                "(6,21): warning JADNC001: Type 'Item' must implement IIdentifiable<TId> when using ResourceAttribute to auto-generate ASP.NET controllers");

        runResult.Should().NotHaveProducedSourceCode();
    }

    [Fact]
    public void Adds_nullable_enable_for_nullable_reference_ID_type()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    #nullable enable

                    [Resource]
                    public sealed class Item : Identifiable<string?>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCodeContaining(@"#nullable enable");
    }

    [Fact]
    public void Can_generate_for_custom_namespace()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode(@"
                    [Resource(ControllerNamespace = ""Some.Path.To.Generate.Code.In"")]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using ExampleApi.Models;

namespace Some.Path.To.Generate.Code.In
{
    public sealed partial class ItemsController : JsonApiController<Item, long>
    {
        public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Item, long> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
");
    }

    [Fact]
    public void Can_generate_for_top_level_namespace()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("TopLevel")
            .WithCode(@"
                    [Resource]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using TopLevel;

namespace Controllers
{
    public sealed partial class ItemsController : JsonApiController<Item, long>
    {
        public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Item, long> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
");
    }

    [Fact]
    public void Can_generate_for_global_namespace()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .WithCode(@"
                    [Resource]
                    public sealed class Item : Identifiable<long>
                    {
                        [Attr]
                        public int Value { get; set; }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();

        runResult.Should().HaveProducedSourceCode(@"using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

public sealed partial class ItemsController : JsonApiController<Item, long>
{
    public ItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Item, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
");
    }

    [Fact]
    public void Generates_unique_file_names_for_duplicate_resource_name_in_different_namespaces()
    {
        // Arrange
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ControllerSourceGenerator());

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .WithCode(@"
                    namespace The.First.One
                    {
                        [Resource]
                        public sealed class Item : Identifiable<long>
                        {
                            [Attr]
                            public int Value { get; set; }
                        }
                    }

                    namespace The.Second.One
                    {
                        [Resource]
                        public sealed class Item : Identifiable<long>
                        {
                            [Attr]
                            public int Value { get; set; }
                        }
                    }")
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation outputCompilation, out _);

        // Assert
        inputCompilation.GetDiagnostics().Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        runResult.Should().NotHaveDiagnostics();
        runResult.Results.Should().HaveCount(1);

        GeneratorRunResult generatorResult = runResult.Results[0];
        generatorResult.GeneratedSources.Should().HaveCount(2);

        generatorResult.GeneratedSources[0].HintName.Should().Be("ItemsController.g.cs");
        generatorResult.GeneratedSources[1].HintName.Should().Be("ItemsController2.g.cs");
    }
}
