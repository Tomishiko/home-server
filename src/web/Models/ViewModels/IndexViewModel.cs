namespace web.ViewModels;

public class IndexPageViewModel
{
    public required IAsyncEnumerable<core.Models.FileMeta> Files { get; init; }
    public required bool IsPrivate { get; init; }
}
