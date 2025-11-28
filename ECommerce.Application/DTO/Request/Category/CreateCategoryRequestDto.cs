namespace ECommerce.Application.DTO.Request.Category;

public record CreateCategoryRequestDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}