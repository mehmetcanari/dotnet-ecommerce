public record CreateOrderItemDto
{
    public required int Quantity {get;set;}
    public required int ProductId {get;set;}
    public required int UserId {get;set;}
}