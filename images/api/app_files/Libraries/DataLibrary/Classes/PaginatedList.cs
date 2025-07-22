namespace DataLibrary.Classes
{
    /// <summary>
    /// Represents navigation links for paginated results, including next and previous page references.
    /// </summary>
    /// <remarks>
    /// The <see cref="Links"/> class is used to encapsulate URIs or identifiers for navigating to the next and previous pages in a paginated list.
    /// </remarks>
    public class Links(string? Next, string? Previous)
    {
        /// <summary>
        /// The reference to the next page in the paginated list.
        /// </summary>
        /// <remarks>
        /// Contains the identifier or URI for the next page, or null if there is no next page.
        /// </remarks>
        public string? Next { get; set; } = Next;

        /// <summary>
        /// The reference to the previous page in the paginated list.
        /// </summary>
        /// <remarks>
        /// Contains the identifier or URI for the previous page, or null if there is no previous page.
        /// </remarks>
        public string? Previous { get; set; } = Previous;

        /// <summary>
        /// Initializes a new instance of the <see cref="Links"/> class with no next or previous page references.
        /// </summary>
        public Links() : this(null, null) { }
    }

    /// <summary>
    /// Represents a paginated list of results with navigation links and total count information.
    /// </summary>
    /// <remarks>
    /// The <see cref="PaginatedList{T}"/> class provides a way to paginate query results, including total count, total pages, and navigation links for next and previous pages.
    /// </remarks>
    public class PaginatedList<T>
    {
        /// <summary>
        /// The total number of items in the data source.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The total number of pages available based on the data source and page size.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The list of results for the current page.
        /// </summary>
        public List<T> Results { get; set; } = [];

        /// <summary>
        /// Navigation links for the paginated results.
        /// </summary>
        /// <remarks>
        /// Contains references to the next and previous pages, if available.
        /// </remarks>
        public Links Links { get; set; } = new Links();

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class using the specified source, page, and limit.
        /// </summary>
        /// <param name="source">The queryable data source to paginate.</param>
        /// <param name="page">The current page number (1-based).</param>
        /// <param name="limit">The maximum number of items per page.</param>

        public PaginatedList(IQueryable<T> source, int page, int limit)
        {
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)limit);

            // Ensure page is at least 1
            page = page < 1 ? 1 : page;

            // If page is out of range, return empty result and no links
            if (page > TotalPages || TotalPages == 0)
            {
                Results = [];
                return;
            }

            // Calculate the correct offset
            var offset = (page - 1) * limit;

            Results = [.. source.Skip(offset).Take(limit)];

            // Set previous and next links
            if (page > 1)
                Links.Previous = (page - 1).ToString();

            if (page < TotalPages)
                Links.Next = (page + 1).ToString();

        }
    }
}
