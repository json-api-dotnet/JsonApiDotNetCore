using System.Text.Json;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace GettingStarted.Controllers;

partial class BooksController
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceService<Book, int> _resourceService;
    private readonly IResponseModelAdapter _responseModelAdapter;

    [ActivatorUtilitiesConstructor]
    public BooksController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Book, int> resourceService,
        IResponseModelAdapter responseModelAdapter)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
        _options = options;
        _resourceService = resourceService;
        _responseModelAdapter = responseModelAdapter;
    }

    // Here we can use all the JSON:API features in the export, such as includes, sorting and sparse fieldsets.
    // http://localhost:14141/api/books/exportZip?include=author&sort=author.name&fields[books]=title,author

    [HttpGet("exportZip")]
    public async Task<IActionResult> GetZipFileAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Book> books = await _resourceService.GetAsync(cancellationToken);

        var zipFileBuilder = new ZipFileBuilder();

        await WriteResourcesToZipFileAsync(books, zipFileBuilder);

        Stream zipStream = zipFileBuilder.Build();
        return File(zipStream, "application/zip", "Export.zip");
    }

    private async Task WriteResourcesToZipFileAsync<TResource>(IEnumerable<TResource> resources, ZipFileBuilder zipFileBuilder)
        where TResource : class, IIdentifiable
    {
        const string fileName = "books.json";
        string json = ResourcesToJson(resources);

        await zipFileBuilder.IncludeFileAsync(fileName, json);
    }

    private string ResourcesToJson<TResource>(IEnumerable<TResource> resources)
        where TResource : class, IIdentifiable
    {
        Document document = _responseModelAdapter.Convert(resources);
        return JsonSerializer.Serialize(document, _options.SerializerOptions);
    }
}
