using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RoundsApp.Models;

/// <summary>
/// Represents a drink.
/// </summary>
public class Drink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Producer { get; set; }

    [Required]
    public DrinkType Type { get; set; } = DrinkType.Beer;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal AlcoholContent { get; set; } = 0.0m; // in percentage

    [Required]
    [ForeignKey("CreatedBy")]
    public Guid CreatedById { get; set; }

    public ApplicationUser? CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UpdatedBy")]
    public Guid? UpdatedById { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
