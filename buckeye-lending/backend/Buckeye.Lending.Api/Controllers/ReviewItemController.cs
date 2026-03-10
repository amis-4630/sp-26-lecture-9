using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Models;
using Buckeye.Lending.Api.Dtos;

namespace Buckeye.Lending.Api.Controllers;

[ApiController]
[Route("api/review-queue")]
public class ReviewQueueController : ControllerBase
{
    private readonly LendingContext _context;
    private const string CurrentOfficerId = "default-officer";

    public ReviewQueueController(LendingContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ReviewQueue>> GetQueue()
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<ActionResult<ReviewItem>> AddToQueue(AddToQueueRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPut("{itemId}")]
    public async Task<ActionResult<ReviewItem>> UpdateItem(int itemId, UpdateItemRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearQueue()
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}