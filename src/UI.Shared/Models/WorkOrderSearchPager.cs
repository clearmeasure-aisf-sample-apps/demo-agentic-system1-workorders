using System.Globalization;

namespace ClearMeasure.Bootcamp.UI.Shared.Models;

/// <summary>One page of results and the numbers needed to describe it.</summary>
public sealed record SearchResultPage(WorkOrderSearchResultRow[] Rows, int Page, int PageCount, int First, int Last, int Total)
{
    /// <summary>"Showing a–b of M work orders".</summary>
    public string RangeText => Total == 0
        ? "Showing 0 of 0 work orders"
        : string.Create(CultureInfo.InvariantCulture, $"Showing {First}–{Last} of {Total} {(Total == 1 ? "work order" : "work orders")}");

    /// <summary>"N of M": rows on this page of the filtered total.</summary>
    public string CountText => string.Create(CultureInfo.InvariantCulture, $"{Rows.Length} of {Total}");

    /// <summary>
    /// The page numbers to offer: the first, the last, and the two either side of the current page. Every number
    /// of a long result widened the page past the screen.
    /// </summary>
    public IEnumerable<int> PageNumbers => Enumerable.Range(1, PageCount)
        .Where(n => n == 1 || n == PageCount || Math.Abs(n - Page) <= 2);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < PageCount;
}

/// <summary>Client-side paging of search results (25 per page).</summary>
public static class WorkOrderSearchPager
{
    /// <summary>Rows per page.</summary>
    private const int PageSize = 25;

    /// <summary>The requested page, clamped to the available pages.</summary>
    public static SearchResultPage GetPage(IReadOnlyList<WorkOrderSearchResultRow> rows, int page)
    {
        var pageCount = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)PageSize));
        var current = Math.Clamp(page, 1, pageCount);
        var slice = rows.Skip((current - 1) * PageSize).Take(PageSize).ToArray();
        var first = slice.Length == 0 ? 0 : ((current - 1) * PageSize) + 1;
        return new SearchResultPage(slice, current, pageCount, first, first + slice.Length - Math.Sign(slice.Length), rows.Count);
    }
}
