namespace core.Models;

public record File(string UUID, string Name, ulong Size, string Ext, string? Owner = null, uint? Id = null);
