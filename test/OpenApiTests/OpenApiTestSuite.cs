using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace OpenApiTests;

public abstract class OpenApiTestSuite<TStartup, TDbContext> : IClassFixture<OpenApiTestContext<TStartup, TDbContext>>
    where TStartup : class
    where TDbContext : DbContext
{
    private readonly OpenApiTestContext<TStartup, TDbContext> _testContext;
    private readonly bool _isFirstTestRunInTestSuite;

    protected OpenApiTestSuite(OpenApiTestContext<TStartup, TDbContext> testContext)
    {
        _testContext = testContext;
        _isFirstTestRunInTestSuite = !testContext.LazyDocument.IsValueCreated;
    }

    protected void UseController<TController>()
        where TController : ControllerBase
    {
        if (_isFirstTestRunInTestSuite)
        {
            _testContext.UseController<TController>();
        }
    }

    protected async Task<JsonElement> GetDocumentAsync()
    {
        JsonElement document = await _testContext.LazyDocument.Value;

        if (_isFirstTestRunInTestSuite)
        {
            await WriteSwaggerDocumentToFileAsync(document);
        }

        return document;
    }

    private async Task WriteSwaggerDocumentToFileAsync(JsonElement document)
    {
        string pathToTestSuiteDirectory = GetTestSuitePath();
        string pathToDocument = Path.Join(pathToTestSuiteDirectory, "swagger.json");
        string json = document.ToString();
        await File.WriteAllTextAsync(pathToDocument, json);
    }

    private static string GetTestSuitePath()
    {
        string pathToSolutionTestDirectory = Path.Combine(Environment.CurrentDirectory, "../../../../");
        pathToSolutionTestDirectory = Path.GetFullPath(pathToSolutionTestDirectory);

        string pathToCurrentNamespaceRelativeToTestDirectory = Path.Combine(typeof(TStartup).Namespace!.Split('.'));

        return Path.Join(pathToSolutionTestDirectory, pathToCurrentNamespaceRelativeToTestDirectory);
    }
}
