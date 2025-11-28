namespace ECommerce.Shared.DTO.Request.Category;

public record UpdateCategoryRequestDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}