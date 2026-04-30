namespace core.Interfaces;

public interface IPhysicalFileWriter : IDisposable
{
    ValueTask Write(ReadOnlyMemory<byte> blob, long offset,
                            CancellationToken ct);

    bool IsClosed { get; }
    void FlushToDisk();

}
