namespace core.Models;

public sealed record FileMeta(string UUID,
                       string Name,
                       long Size,
                       string Ext,
                       string? Owner = null,
                       long? Id = null);
