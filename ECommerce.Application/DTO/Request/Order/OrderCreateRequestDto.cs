using ECommerce.Domain.Model;

namespace ECommerce.Application.DTO.Request.Order;

public record OrderCreateRequestDto
{
    public required PaymentCard PaymentCard { get; set; }
}