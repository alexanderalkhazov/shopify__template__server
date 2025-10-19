using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("entities")]
public class Entity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column("code")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    [Column("priority")]
    public int Priority { get; set; } = 1;

    [Column("price", TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; } = 0;

    [Column("percentage", TypeName = "decimal(5,2)")]
    public decimal? Percentage { get; set; }

    [Column("is_featured")]
    public bool IsFeatured { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("tags")]
    [MaxLength(500)]
    public string? Tags { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; } // JSON column for flexible data

    [Column("external_id")]
    [MaxLength(100)]
    public string? ExternalId { get; set; }

    [Column("category")]
    [MaxLength(100)]
    public string? Category { get; set; }

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }
}