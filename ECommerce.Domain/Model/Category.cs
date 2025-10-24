namespace ECommerce.Domain.Model;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}
