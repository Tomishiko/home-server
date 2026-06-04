namespace core.Models;

public record FileHandshakeResponseDto(string Uuid,
                                       int PartSize,
                                       long WindowStart,
                                       uint Bitfield);
