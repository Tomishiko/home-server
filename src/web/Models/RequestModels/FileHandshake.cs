namespace web.Models;

public sealed record FileHandshake(uint expectedPartSize,
                            string fileName,
                            long fileSize,
                            uint totalParts);
