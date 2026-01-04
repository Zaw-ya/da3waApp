using Da3wa.Application.DTOs;
using Da3wa.Domain.Entities;

namespace Da3wa.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, string? createdById = null);
        Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string userId);
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        Task<bool> ChangeUserPasswordAsync(string userId, string newPassword);
        Task<bool> AssignRoleAsync(string userId, string role);
        Task<bool> RemoveRoleAsync(string userId, string role);
        Task<IEnumerable<string>> GetAllRolesAsync();
        Task<bool> ToggleUserActiveStatusAsync(string userId);
        Task<bool> ToggleUserApprovalStatusAsync(string userId);
    }
}

