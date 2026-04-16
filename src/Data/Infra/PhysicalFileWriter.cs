using Microsoft.Win32.SafeHandles;

namespace Data.Infra;

public class PhysicalFileWriter : IDisposable
{
    private readonly SafeFileHandle _fileHandle;

    public PhysicalFileWriter(string filePath, long fileSize)
    {

        _fileHandle = File.OpenHandle(
                        filePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.Write,
                        FileOptions.Asynchronous | FileOptions.RandomAccess, //| FileOptions.WriteThrough,
                        preallocationSize: fileSize);

    }

    public ValueTask Write(ReadOnlyMemory<byte> blob, long offset,
                           CancellationToken ct)
    {
        return RandomAccess.WriteAsync(_fileHandle, blob, offset, ct);

    }

    public void Dispose()
    {
        _fileHandle.Dispose();
    }
}
