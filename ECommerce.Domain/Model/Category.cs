namespace ECommerce.Domain.Model;

public class Category
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
