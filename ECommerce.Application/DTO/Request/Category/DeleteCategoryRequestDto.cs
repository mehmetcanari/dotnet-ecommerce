namespace ECommerce.Application.DTO.Request.Category;

public record DeleteCategoryRequestDto
{
    public required int CategoryId { get; set; }
}