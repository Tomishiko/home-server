using System.Collections.Immutable;
using core.Models;

namespace web.ViewModels;

public class LogsPartialTableViewModel
{
    public ImmutableArray<LogDto> Logs { get; set; }
    public string? Cursor { get; set; }
    public bool BtnDisabled { get; set; }
    /// <summary>
    /// Needed for proper subpage loading.
    /// If this partial view rendered from parent view button is not needed,
    /// but if this view rendered as a response to load more button click, then button should be rendered with hx-swap-oob.
    /// </summary>
    public bool HideLoadMoreBtn { get; set; }
}
