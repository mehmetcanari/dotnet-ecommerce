using ECommerce.Application.DTO.Response.Product;

public record CategoryResponseDto
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public ICollection<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();
}
