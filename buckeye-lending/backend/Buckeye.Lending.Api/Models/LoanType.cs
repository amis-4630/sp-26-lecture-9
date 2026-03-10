using System.ComponentModel.DataAnnotations;

namespace Buckeye.Lending.Api.Models;

/// <summary>
/// Lookup / reference entity for the kind of loan (Mortgage, Auto, Personal, Business).
/// </summary>
public class LoanType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Maximum term in months for this loan type.</summary>
    public int MaxTermMonths { get; set; }

    // Navigation — one loan type can appear on many applications
    public List<LoanApplication> LoanApplications { get; set; } = [];
}
