using HabitTracker.Application.DTOs.AdminDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // <-- DİKKAT: JWT Token oluştururken "Admin" rolü vermen gerekir.
    // Şimdilik test için Authorize'ı açık bırakıyorum ama canlıda mutlaka Roles ekle.
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAuthService _authService;
        public AdminController(IAdminService adminService, IAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        // --- KULLANICILAR ---

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            
            await _adminService.DeleteUserAsync(id);
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
            
           
            
        }

        // --- ARKADAŞLIKLAR ---

        [HttpGet("friendships")]
        public async Task<IActionResult> GetAllFriendships()
        {
            var friendships = await _adminService.GetAllFriendshipsAsync();
            return Ok(friendships);
        }

        [HttpDelete("friendships/{id}")]
        public async Task<IActionResult> DeleteFriendship(int id)
        {          
            await _adminService.DeleteFriendshipAsync(id);
            return Ok(new { message = "Arkadaşlık kaydı silindi." });
                        
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto request)
        {
            // Güvenlik kontrolü: URL'deki ID ile Body'deki ID uyuşuyor mu?
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID uyuşmazlığı." });
            } 
            
           await _adminService.UpdateUserAsync(request);
           return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
            
             
        }
        [HttpGet("activities")]
        public async Task<IActionResult> GetRecentActivities()
        {            
            var activities = await _adminService.GetRecentActivitiesAsync();
            return Ok(activities);                      
        }

        [HttpGet("habits")]
        public async Task<IActionResult> GetAllHabits()
        {
            var allHabits= await _adminService.GetAllHabitsAsync();
            return Ok(allHabits);
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetAllSystemLogs()
        {
            var allSystemLogs=await _adminService.GetSystemLogsAsync();
            return Ok(allSystemLogs);
        }
    }
}

