namespace BlazorBoilerplate.Shared.Models
{
    /// <summary>
    /// Standardized details including in paginated responses
    /// </summary>
    public class PaginationDetails
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int CollectionSize { get; set; }
    }
}
