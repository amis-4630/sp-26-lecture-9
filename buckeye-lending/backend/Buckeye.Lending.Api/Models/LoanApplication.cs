using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buckeye.Lending.Api.Models;

public class LoanApplication
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required, Column(TypeName = "decimal(12,2)")]
    public decimal LoanAmount { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal AnnualIncome { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Pending";

    [Range(1, 5)]
    public int RiskRating { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Foreign key — which applicant filed this application
    public int ApplicantId { get; set; }
    public Applicant? Applicant { get; set; }

    // Foreign key — what type of loan (replaces the old string LoanType)
    public int LoanTypeId { get; set; }
    public LoanType? LoanType { get; set; }

    // Navigation — one application can have many payments and notes
    public List<LoanPayment> Payments { get; set; } = [];
    public List<LoanNote> LoanNotes { get; set; } = [];
}
