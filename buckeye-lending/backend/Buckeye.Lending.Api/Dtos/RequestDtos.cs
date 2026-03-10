using System.ComponentModel.DataAnnotations;

namespace Buckeye.Lending.Api.Dtos;

public class AddToQueueRequest
{
    public int LoanApplicationId { get; set; }

    [Range(1, 5)]
    public int Priority { get; set; } = 3;
}

public class UpdateItemRequest
{
    public int Priority { get; set; }
    public string? Notes { get; set; }
}