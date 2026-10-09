namespace ApiEcommerce.Models.Dtos
{
    public class PaginatedResponseDTO<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public ICollection<T> Items { get; set; } = new List<T>();
    }
}
