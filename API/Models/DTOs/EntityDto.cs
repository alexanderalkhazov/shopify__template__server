namespace API.Models.DTOs;

/// <summary>
/// Data Transfer Object for Entity - used for API responses
/// </summary>
public class EntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public decimal? Price { get; set; }
    public int Quantity { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public string? Tags { get; set; }
    public string? Metadata { get; set; }
    public string? ExternalId { get; set; }
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Simplified Entity DTO for list views
/// </summary>
public class EntitySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public decimal? Price { get; set; }
    public bool IsActive { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating new entities
/// </summary>
public class CreateEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int Priority { get; set; } = 1;
    public decimal? Price { get; set; }
    public int Quantity { get; set; } = 0;
    public decimal? Percentage { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }
    public string? Metadata { get; set; }
    public string? ExternalId { get; set; }
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating existing entities
/// </summary>
public class UpdateEntityDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public string? Status { get; set; }
    public int? Priority { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public decimal? Percentage { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
    public string? Tags { get; set; }
    public string? Metadata { get; set; }
    public string? ExternalId { get; set; }
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UpdatedBy { get; set; }
}