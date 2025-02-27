namespace OnlineStoreWeb.API.DTO.Request.Order;

public record OrderCreateDto
{
    public required int AccountId {get;set;}
    public required string ShippingAddress {get;set;}
    public required string BillingAddress {get;set;}
    public required PaymentMethod PaymentMethod {get;set;}
}