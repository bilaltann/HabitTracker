using HabitTracker.Application.DTOs.AdminDTOs;
using HabitTracker.Application.DTOs.HabitDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IFriendshipRepository _friendshipRepo;
        private readonly IRepository<SystemLog> _systemLogRepo;


        // --- YENİ EKLENEN REPOLAR (Temizlik için lazım) ---
        private readonly IRepository<Habit> _habitRepo;
        private readonly IRepository<HabitInvitation> _invitationRepo;
        private readonly IRepository<UserBadge> _userBadgeRepo;
        private readonly IHabitLogRepository _habitLogRepo;

        public AdminService(
            IRepository<User> userRepo,
            IFriendshipRepository friendshipRepo,
            IRepository<Habit> habitRepo,
            IRepository<HabitInvitation> invitationRepo,
            IRepository<UserBadge> userBadgeRepo,
            IHabitLogRepository habitLogRepo,
            IRepository<SystemLog> systemLogRepo)
        {
            _userRepo = userRepo;
            _friendshipRepo = friendshipRepo;
            _habitRepo = habitRepo;
            _invitationRepo = invitationRepo;
            _userBadgeRepo = userBadgeRepo;
            _habitLogRepo = habitLogRepo;
            _systemLogRepo = systemLogRepo;
        }
        public async Task DeleteFriendshipAsync(int friendshipId)
        {
            var friendship = await _friendshipRepo.GetByIdAsync(friendshipId);
            if (friendship == null) throw new Exception("Arkadaşlık kaydı bulunamadı.");

            await _friendshipRepo.DeleteAsync(friendshipId);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // --- MANUEL CASCADE DELETE (Temizlik İşlemi) ---

            // 1. Arkadaşlıkları Sil (Hem gönderen hem alan olarak)
            await _friendshipRepo.DeleteAllAsync(f => f.RequesterId == userId || f.AddresseeId == userId);

            // 2. Alışkanlık Davetlerini Sil
            await _invitationRepo.DeleteAllAsync(i => i.SenderId == userId || i.ReceiverId == userId);

            // 3. Kullanıcının Rozetlerini Sil
            await _userBadgeRepo.DeleteAllAsync(ub => ub.UserId == userId);

            // 4. Alışkanlıkları ve Logları Sil
            // Not: Alışkanlığı silince loglar otomatik silinebilir (Cascade varsa), 
            // ama garanti olsun diye önce kullanıcının habitlerini bulup loglarını silebiliriz.
            // Ancak Habit -> Log ilişkisi genelde Cascade'dir. Direkt Habit silmek yeterli olabilir.
            // Biz yine de Habit tablosunu temizleyelim:
            var userHabits = await _habitRepo.GetAllAsync(); // Filtreleme repoda yoksa burada yaparız
            var myHabits = userHabits.Where(h => h.UserId == userId).ToList();

            foreach (var habit in myHabits)
            {
                // Önce o alışkanlığın loglarını sil
                await _habitLogRepo.DeleteAllAsync(l => l.HabitId == habit.Id);
                // Sonra alışkanlığı sil
                await _habitRepo.DeleteAsync(habit.Id);
            }

            // 5. SON OLARAK: Kullanıcıyı Sil
            await _userRepo.DeleteAsync(userId);
        }

        public  async Task<IEnumerable<AdminFriendshipListDto>> GetAllFriendshipsAsync()
        {
            // Tüm arkadaşlıkları çek (Include işlemini Repository'de yapman gerekebilir)
            // Eğer IFriendshipRepository içinde GetAllWithDetailsAsync gibi bir metodun yoksa,
            // Generic GetAllAsync ile çekip kullanıcıları ayrıca eşleştirmemiz gerekebilir.

            // YÖNTEM: Repository'ye özel bir metot eklemiş varsayıyorum veya 
            // Generic Repository'den tümünü çekip Join yapacağız.

            var friendships = await _friendshipRepo.GetAllAsync();
            var users = await _userRepo.GetAllAsync(); // İsimleri bulmak için tüm kullanıcılar

            var result = from f in friendships
                         join u1 in users on f.RequesterId equals u1.Id
                         join u2 in users on f.AddresseeId equals u2.Id
                         select new AdminFriendshipListDto
                         {
                             Id = f.Id,
                             RequesterName = u1.Name,
                             AddresseeName = u2.Name,
                             Status = f.Status.ToString(),
                             CreatedDate = f.CreatedDate
                         };

            return result.OrderByDescending(u => u.Id).ToList();
        }

        public async Task<IEnumerable<HabitDto>> GetAllHabitsAsync()
        {
            var allHabits = await _habitRepo.GetAllAsync();

            return allHabits.Select(u => new HabitDto
            {
              Id=u.Id,
              Name = u.Name,
            }).ToList();
        }

        public async Task<IEnumerable<AdminUserListDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();

            return users.Select(u => new AdminUserListDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Level = u.Level,
                CurrentPoints = u.CurrentPoints,
                Role= u.Role,
            }).OrderByDescending(u => u.Id).ToList();
        }

        public async Task<IEnumerable<AdminActivityDto>> GetRecentActivitiesAsync()
        {
            // Son 10 hareketi çekelim
            var logs = await _habitLogRepo.GetRecentLogsWithDetailsAsync(10);

            return logs.Select(log => new AdminActivityDto
            {
                // Kullanıcı Adı ve Alışkanlık Adı
                Title = $"{log.Habit.User.Name} bir hedefi tamamladı!",

                // Alışkanlık Detayı
                Description = $"{log.Habit.Name} ({log.Habit.Category})",

                // Tarih
                Date = log.CreatedDate, // Logun oluşturulma zamanı

                // Frontend'de yeşil tik çıkması için 'success' gönderiyoruz
                Type = "success"
            }).ToList();
        }

        public async Task<IEnumerable<SystemLog>> GetSystemLogsAsync()
        {
            var logs = await _systemLogRepo.GetAllAsync();
            // En son hatalar en üstte görünsün
            return logs.OrderByDescending(l => l.CreatedDate).ToList();
        }

        public async Task UpdateUserAsync(AdminUpdateUserDto updateDto)
        {
            // 1. Kullanıcıyı bul
            var user = await _userRepo.GetByIdAsync(updateDto.Id);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 2. E-posta değiştiyse, başkası kullanıyor mu kontrol et
            if (updateDto.Email != user.Email)
            {
                var allUsers = await _userRepo.GetAllAsync();
                if (allUsers.Any(u => u.Email == updateDto.Email && u.Id != updateDto.Id))
                {
                    throw new Exception("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
                }
            }

            // 3. Verileri güncelle
            user.Name = updateDto.Name;
            user.Email = updateDto.Email;
            user.Role = updateDto.Role; // Admin yetkisiyle rolü de güncelliyoruz

            // 4. Kaydet
            await _userRepo.UpdateAsync(user);
        }
    }
}
