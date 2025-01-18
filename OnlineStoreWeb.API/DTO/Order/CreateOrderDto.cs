public record CreateOrderDto
{
    public int UserId {get;set;}   
    public string ShippingAddress {get;set;}
    public string PaymentMethod {get;set;}
}
