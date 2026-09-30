using Library.Application.Dtos;
using Library.Applictation.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAllUsersAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");

            if (!isStaff && currentUserId != id.ToString())
            {
                return Forbid();
            }

            return Ok(await _userService.GetUserByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");

            if (!isStaff && currentUserId == id.ToString())
            {
                return Forbid();
            }

            return Ok(await _userService.UpdateUserAsync(id, request));
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != id.ToString())
                return Forbid(); // только сам пользователь

            await _userService.ChangePasswordAsync(id, request);
            return NoContent();
        }

        [HttpPatch("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request)
        {
            return Ok(await _userService.UpdateRoleAsync(id, request));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(Guid id)
        {
            await _userService.RestoreUserAsync(id);
            return NoContent();
        }
    }
}
