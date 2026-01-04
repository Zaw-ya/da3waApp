using AutoMapper;
using Da3wa.Application.DTOs;
using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain;
using Da3wa.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, string? createdById = null)
        {
            var user = _mapper.Map<ApplicationUser>(createUserDto);
            
            // Set defaults for nullable string properties
            user.Address = createUserDto.Address ?? string.Empty;
            user.PrimaryContactNo = createUserDto.PrimaryContactNo ?? string.Empty;
            user.SecondaryContactNo = createUserDto.SecondaryContactNo ?? string.Empty;

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Assign role
            if (!string.IsNullOrWhiteSpace(createUserDto.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(createUserDto.Role);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, createUserDto.Role);
                }
            }

            return await MapToUserDtoAsync(user);
        }

        public async Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(updateUserDto.Id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {updateUserDto.Id} not found.");
            }

            // Map DTO to existing user entity (only updates non-null properties)
            _mapper.Map(updateUserDto, user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            // Update role if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                var roleExists = await _roleManager.RoleExistsAsync(updateUserDto.Role);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, updateUserDto.Role);
                }
            }

            return await MapToUserDtoAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return await MapToUserDtoAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.City)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(await MapToUserDtoAsync(user));
            }

            return userDtos;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var userWithCity = await _unitOfWork.ApplicationUsers.GetQueryable()
                    .Include(u => u.City)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);
                
                if (userWithCity != null)
                {
                    userDtos.Add(await MapToUserDtoAsync(userWithCity));
                }
            }

            return userDtos;
        }

        public async Task<bool> ChangeUserPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                return false;
            }

            var isInRole = await _userManager.IsInRoleAsync(user, role);
            if (isInRole)
            {
                return true; // Already in role
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var isInRole = await _userManager.IsInRoleAsync(user, role);
            if (!isInRole)
            {
                return true; // Not in role, consider it success
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            var roles = _roleManager.Roles.Select(r => r.Name!);
            return await Task.FromResult(roles);
        }

        public async Task<bool> ToggleUserActiveStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = !user.IsActive;
            user.ModifiedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ToggleUserApprovalStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.Approved = !user.Approved;
            user.ModifiedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        private async Task<UserDto> MapToUserDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            // Map basic properties using AutoMapper
            var userDto = _mapper.Map<UserDto>(user);
            
            // Add roles (cannot be mapped automatically as they come from UserManager)
            userDto.Role = roles.FirstOrDefault();
            userDto.Roles = roles.ToList();
            
            return userDto;
        }
    }
}

