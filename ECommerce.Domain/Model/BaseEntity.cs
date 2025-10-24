using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Domain.Model;

public abstract class BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow.ToUniversalTime();
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public bool IsDeleted { get; private set; } = false;

    public void Delete()
    {
        IsDeleted = true;
        UpdatedOn = DateTime.UtcNow;
    }
}
