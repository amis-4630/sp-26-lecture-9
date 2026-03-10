using Microsoft.AspNetCore.Mvc;
using Buckeye.Lending.Api.Models;
using System.Runtime.CompilerServices;
using Buckeye.Lending.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Buckeye.Lending.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // sets the base URI to `/api/loanapplications`
public class LoanApplicationsController : ControllerBase
{
    // In-memory data — in real app, this is a database
    private readonly LendingContext _context;

    public LoanApplicationsController(LendingContext context)
    {
        _context = context;
    }

    // GET: api/LoanApplications?loanTypeId=1&minAmount=100000
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanApplication>>> GetAll(
        [FromQuery] int? loanTypeId,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] string? search)
    {
        var query = _context.LoanApplications
            .Include(l => l.Applicant)
            .Include(l => l.LoanType)
            .AsQueryable();

        if (loanTypeId.HasValue)
            query = query.Where(l => l.LoanTypeId == loanTypeId.Value);

        if (minAmount.HasValue)
            query = query.Where(l => l.LoanAmount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(l => l.LoanAmount <= maxAmount.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.ApplicantName.Contains(search, StringComparison.OrdinalIgnoreCase));

        return Ok(await query.ToListAsync());
    }

    // GET: api/LoanApplications/2
    [HttpGet("{id}")]
    public async Task<ActionResult<LoanApplication>> GetById(int id)
    {
        var app = await _context.LoanApplications
            .Include(l => l.Applicant)
            .Include(l => l.LoanType)
            .Include(l => l.Payments)
            .Include(l => l.LoanNotes)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (app == null)
            throw new KeyNotFoundException($"Loan application with ID {id} not found");
        return Ok(app);
    }

    // POST: api/LoanApplications
    [HttpPost]
    public async Task<ActionResult<LoanApplication>> Create(LoanApplication application)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(application.ApplicantName))
            throw new ArgumentException("Applicant name is required", nameof(application.ApplicantName));

        if (application.LoanAmount <= 0)
            throw new ArgumentException("Loan amount must be positive", nameof(application.LoanAmount));

        // Set server-controlled fields
        application.Status = "Pending Review";
        application.SubmittedDate = DateTime.Now;

        _context.LoanApplications.Add(application);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = application.Id },
            application
        );
    }

    // PUT: api/LoanApplications/2
    [HttpPut("{id}")]
    public async Task<ActionResult<LoanApplication>> Update(int id, LoanApplication updated)
    {
        var existing = await _context.LoanApplications.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Loan application with ID {id} not found");

        // Validate
        if (string.IsNullOrWhiteSpace(updated.ApplicantName))
            throw new ArgumentException("Applicant name is required", nameof(updated.ApplicantName));

        if (updated.LoanAmount <= 0)
            throw new ArgumentException("Loan amount must be positive", nameof(updated.LoanAmount));

        // Update allowed fields
        existing.ApplicantName = updated.ApplicantName;
        existing.LoanAmount = updated.LoanAmount;
        existing.LoanType = updated.LoanType;
        existing.AnnualIncome = updated.AnnualIncome;
        existing.ApplicantId = updated.ApplicantId;
        existing.LoanTypeId = updated.LoanTypeId;
        // Don't update: Id, Status, SubmittedDate (server-controlled)

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // DELETE: api/LoanApplications/3
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var app = await _context.LoanApplications.FindAsync(id);
        if (app == null)
            throw new KeyNotFoundException($"Loan application with ID {id} not found");

        _context.LoanApplications.Remove(app);
        await _context.SaveChangesAsync();
        return NoContent();  // 204
    }
}
