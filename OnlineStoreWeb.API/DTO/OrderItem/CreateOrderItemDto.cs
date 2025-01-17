public record CreateOrderItemDto
{
    public int Quantity {get;set;}
    public int ProductId {get;set;}
    public int UserId {get;set;}
}