namespace core.Models;

public record FileHandleConfig(string path,
                               FileMode fileMode,
                               FileAccess FileAccess,
                               FileShare fileShare,
                               long preallocationSize);
