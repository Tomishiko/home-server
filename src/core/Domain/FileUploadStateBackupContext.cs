using System.Buffers;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace core.Domain;


public class FileUploadStateBackupContext
{
    public uint Bitfield { get; set; }
    public Guid Id { get; set; }
    public int PartsWritten { get; set; }

}
