using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiTests.MixedControllers;

[Route("fileTransfers")]
[Tags("fileTransfers")]
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
    [EndpointDescription("Uploads a file. Returns HTTP 400 if the file is empty.")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
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

    [HttpGet("find", Name = "exists")]
    [HttpHead("find", Name = "tryExists")]
    [EndpointDescription("Returns whether the specified file is available for download.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public IActionResult Exists(string fileName)
    {
        return _inMemoryFileStorage.Files.ContainsKey(fileName) ? Ok() : NotFound();
    }

    [HttpGet(Name = "download")]
    [HttpHead(Name = "tryDownload")]
    [EndpointDescription("Downloads the file with the specified name. Returns HTTP 404 if not found.")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK, BinaryContentType)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public IActionResult Download(string fileName)
    {
        if (_inMemoryFileStorage.Files.TryGetValue(fileName, out byte[]? fileContents))
        {
            return File(fileContents, BinaryContentType);
        }

        return NotFound($"The file '{fileName}' does not exist.");
    }
}
