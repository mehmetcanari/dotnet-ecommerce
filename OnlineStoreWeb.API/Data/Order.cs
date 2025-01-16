public class Order
{
    public int Id {get;set;}
    public Product Product { get; set; }
    public DateTime OrderCreated { get; set; }
    public DateTime OrderUpdated { get; set; }
}