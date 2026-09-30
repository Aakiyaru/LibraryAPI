using Library.Application.Dtos;
using Library.Application.Interfaces;
using Library.Applictation.Dtos;
using Library.Applictation.Interfaces;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Loans)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new InvalidOperationException("Пользователь не найден");
            }

            if(!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Текущий пароль указан неверно");
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ChangePassword(newHash);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Loans)
                .FirstOrDefaultAsync (u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException("Пользователь не найден");
            }

            user.MarkAsDeleted();
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Loans)
                .ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Loans)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException("Пользователь не найден");
            }

            return MapToDto(user);
        }

        public async Task RestoreUserAsync(Guid id)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Loans)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException("Пользователь не найден");
            }

            user.Restore();
            await _context.SaveChangesAsync();
        }

        public async Task<UserDto> UpdateRoleAsync(Guid id, UpdateUserRoleRequest request)
        {
            var user = await _context.Users
                .Include (u => u.Loans)
                .FirstOrDefaultAsync (u => u.Id == id);

            if(user == null)
            {
                throw new KeyNotFoundException("Пользователь не найден");
            }

            user.UpdateRole(request.Role);
            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users
                .Include (u => u.Loans)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException("Пользователь не найден");
            }

            user.UpdateProfile(request.FullName);
            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        private static UserDto MapToDto(Domain.Entities.User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            RegistrationDate = user.RegistrationDate,
            ActiveLoansCount = user.Loans.Count(l => l.ReturnDate == null)
        };
    }
}
