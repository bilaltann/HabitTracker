using HabitTracker.Application.DTOs.AdminDTOs;
using HabitTracker.Application.DTOs.HabitDTOs;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IAdminService
    {
        // Kullanıcı İşlemleri
        Task<IEnumerable<AdminUserListDto>> GetAllUsersAsync();
        Task DeleteUserAsync(int userId);

        // Arkadaşlık İşlemleri
        Task<IEnumerable<AdminFriendshipListDto>> GetAllFriendshipsAsync();
        Task DeleteFriendshipAsync(int friendshipId);

        Task UpdateUserAsync(AdminUpdateUserDto updateDto);
        Task<IEnumerable<AdminActivityDto>> GetRecentActivitiesAsync();

        Task<IEnumerable<HabitDto>> GetAllHabitsAsync();
        Task<IEnumerable<SystemLog>> GetSystemLogsAsync();




    }
}
