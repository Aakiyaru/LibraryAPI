using Library.Application.Dtos;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetAll()
    {
        var loans = await _loanService.GetAllLoansAsync();
        return Ok(loans);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserLoans(Guid userId)
    {
        var loans = await _loanService.GetUserLoansAsync(userId);
        return Ok(loans);
    }

    [HttpPost("borrow")]
    public async Task<IActionResult> Borrow([FromBody] BorrowRequest request)
    {
        try
        {
            var result = await _loanService.BorrowBookAsync(request);
            return CreatedAtAction(nameof(GetUserLoans), new { userId = request.UserId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return([FromBody] ReturnRequest request)
    {
        try
        {
            var result = await _loanService.ReturnBookAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}