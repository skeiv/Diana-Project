using JobSearchApp.API.DTOs;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobSearchApp.API.Services
{
    public interface IUserService
    {
        Task<User?> GetCurrentUser();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> SearchUsersAsync(string query);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role);
    }
} 