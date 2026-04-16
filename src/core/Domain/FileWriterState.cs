using System.Buffers;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace core.Domain;


public record FileWriterMeta(
    long FileSize,
    int PartSize,
    string FileName,
    long OwnerId,
    long TotalFileParts
);

public class FileStateBackupContext : IDisposable
{
    private readonly byte[] _bitfieldBuffer;
    private bool _isDisposed = false;

    public int BitfieldLength { get; }
    public Guid Id { get; }
    public int PartsWritten { get; }


    public ReadOnlyMemory<byte> Bitfield => _isDisposed
        ? Memory<byte>.Empty
        : _bitfieldBuffer.AsMemory(0, BitfieldLength);

    public FileStateBackupContext(int partsWritten,
                                  ReadOnlySpan<byte> bitfield,
                                  Guid id)
    {
        _bitfieldBuffer = ArrayPool<byte>.Shared.Rent(bitfield.Length);
        bitfield.CopyTo(_bitfieldBuffer);
        BitfieldLength = bitfield.Length;
        PartsWritten = partsWritten;
        Id = id;
    }


    public void Dispose()
    {
        if (!_isDisposed)
        {
            ArrayPool<byte>.Shared.Return(_bitfieldBuffer);
            _isDisposed = true;
        }
    }

};
