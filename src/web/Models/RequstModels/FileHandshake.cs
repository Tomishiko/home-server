namespace web.Models;

public record FileHandshake(uint expectedPartSize,string fileName,uint fileSize,uint totalParts);
