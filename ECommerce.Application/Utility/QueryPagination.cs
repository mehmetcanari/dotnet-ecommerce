namespace ECommerce.Application.Utility;

public record QueryPagination
{
    public int Page { get; set; } 
    public int PageSize { get; set; }
}
