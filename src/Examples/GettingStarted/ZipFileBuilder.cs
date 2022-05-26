using System.IO.Compression;

namespace GettingStarted;

internal sealed class ZipFileBuilder
{
    private readonly Stream _zipStream;
    private readonly ZipArchive _zipArchive;

    public ZipFileBuilder()
    {
        _zipStream = new MemoryStream();
        _zipArchive = new ZipArchive(_zipStream, ZipArchiveMode.Update, true);
    }

    public async Task IncludeFileAsync(string fileName, string content)
    {
        ZipArchiveEntry zipEntry = _zipArchive.CreateEntry(fileName);

        await using Stream entryStream = zipEntry.Open();
        await using var writer = new StreamWriter(entryStream);
        await writer.WriteAsync(content);
    }

    public Stream Build()
    {
        _zipArchive.Dispose();
        _zipStream.Seek(0, SeekOrigin.Begin);
        return _zipStream;
    }
}
