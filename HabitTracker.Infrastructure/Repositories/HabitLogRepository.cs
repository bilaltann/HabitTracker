using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure.Repositories
{
    public class HabitLogRepository : Repository<HabitLog>, IHabitLogRepository
    {
        private readonly HabitTrackerDbContext _context;

        public HabitLogRepository(HabitTrackerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<HabitLog?> GetLogByDateAsync(int habitId, DateTime date)
        {
            return await _context.HabitLogs.FirstOrDefaultAsync(a => a.HabitId == habitId && a.CompletedDate == date.Date);
        }

        public async Task<IEnumerable<HabitLog>> GetLogsByUserIdAndDateAsync(int userId, DateTime date)
        {
            return await _context.HabitLogs
                 .Include(l => l.Habit) // Habit tablosuna git
                 .Where(l => l.Habit.UserId == userId && l.CompletedDate.Date == date.Date)
                 .ToListAsync();
        }

        public async Task<IEnumerable<HabitLog>> GetRecentLogsWithDetailsAsync(int count)
        {
            // HabitLogs -> Habit -> User tablosuna erişiyoruz (Include zinciri)
            return await _context.HabitLogs
                .Include(l => l.Habit)
                    .ThenInclude(h => h.User) // Alışkanlığın sahibi olan kullanıcıyı al
                .OrderByDescending(l => l.CreatedDate) // En son eklenen en başta
                .Take(count) // Sadece son X adedi getir (Örn: 10)
                .ToListAsync();
        }
    }
}
