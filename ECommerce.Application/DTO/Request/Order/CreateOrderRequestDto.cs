using ECommerce.Domain.Model;

namespace ECommerce.Application.DTO.Request.Order;

public record CreateOrderRequestDto
{
    public required PaymentCard PaymentCard { get; set; }
}