namespace ECommerce.Application.DTO.Request.Order;

public record OrderCreateRequestDto
{
    public required string ShippingAddress {get;set;}
    public required string BillingAddress {get;set;}
    public required PaymentMethod PaymentMethod {get;set;}
}