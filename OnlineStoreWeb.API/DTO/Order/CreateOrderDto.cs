using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.DTO.Order;

public record OrderCreateDto
{
    public required int UserId {get;set;}
    public required int ProductId {get;set;}
    public required int Quantity {get;set;}
    public required string ShippingAddress {get;set;}
    public required string BillingAddress {get;set;}
    public required PaymentMethod PaymentMethod {get;set;}
}