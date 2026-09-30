using Library.Application.Dtos;
using Library.Applictation.Dtos;

namespace Library.Applictation.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task ChangePasswordAsync(Guid id, ChangePasswordRequest request);
        Task<UserDto> UpdateRoleAsync (Guid id, UpdateUserRoleRequest request);
        Task DeleteUserAsync (Guid id);
        Task RestoreUserAsync (Guid id);
    }
}
