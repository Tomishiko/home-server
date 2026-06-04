using Microsoft.Win32.SafeHandles;
using core.Interfaces;

namespace Data.Infra;

public sealed class PhysicalFileWriterFactory() : IPhysicalFileWriterFactory
{

    public IPhysicalFileWriter Create(string filePath, long preallocationSize)
    {
        return new PhysicalFileWriter(filePath, preallocationSize);
    }
}

internal sealed class PhysicalFileWriter : IPhysicalFileWriter
{
    private readonly SafeFileHandle _fileHandle;

    public bool IsClosed => _fileHandle.IsClosed;

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

    public void FlushToDisk()
    {
        RandomAccess.FlushToDisk(_fileHandle);
    }
}

