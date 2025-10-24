using ECommerce.Application.DTO.Response.Product;

namespace ECommerce.Application.DTO.Response.Category;

public record CategoryResponseDto
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}