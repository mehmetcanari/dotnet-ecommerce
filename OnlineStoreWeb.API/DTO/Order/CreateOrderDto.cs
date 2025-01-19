public record OrderCreateDto
{
    public required int UserId {get;set;}   
    public required string ShippingAddress {get;set;}
    public required string PaymentMethod {get;set;}
}
