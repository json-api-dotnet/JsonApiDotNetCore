using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.NonJsonApiControllers.FileTransfers;

[Route("fileTransfers")]
public sealed class FileTransferController : ControllerBase
{
    private const string BinaryContentType = "application/octet-stream";

    private readonly InMemoryFileStorage _inMemoryFileStorage;

    public FileTransferController(InMemoryFileStorage inMemoryFileStorage)
    {
        ArgumentNullException.ThrowIfNull(inMemoryFileStorage);

        _inMemoryFileStorage = inMemoryFileStorage;
    }

    [HttpPost(Name = "upload")]
    public async Task<IActionResult> UploadAsync(IFormFile? file)
    {
        if (file?.Length > 0)
        {
            byte[] fileContents;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                fileContents = stream.ToArray();
            }

            _inMemoryFileStorage.Files.AddOrUpdate(file.FileName, _ => fileContents, (_, _) => fileContents);
            return Ok($"Received file with a size of {file.Length} bytes.");
        }

        return BadRequest("Empty files cannot be uploaded.");
    }

    [HttpGet("find")]
    [HttpHead("find")]
    public IActionResult Exists(string fileName)
    {
        return _inMemoryFileStorage.Files.ContainsKey(fileName) ? Ok() : NotFound();
    }

    [HttpGet]
    [HttpHead]
    public IActionResult Download(string fileName)
    {
        if (_inMemoryFileStorage.Files.TryGetValue(fileName, out byte[]? fileContents))
        {
            return File(fileContents, BinaryContentType);
        }

        return NotFound($"The file '{fileName}' does not exist.");
    }
}
