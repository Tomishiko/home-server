namespace core.Models;

public readonly record struct UploadPartSuccess(int PartIndex, long BytesWritten);
