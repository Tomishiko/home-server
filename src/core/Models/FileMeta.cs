namespace core.Models;

public record FileMeta(string UUID, string Name, ulong Size, string Ext, string? Owner = null, uint? Id = null);
