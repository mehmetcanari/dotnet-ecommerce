using ECommerce.Domain.Model;

public record OrderCreateRequestDto
{
    public required PaymentCard PaymentCard { get; set; }
}
