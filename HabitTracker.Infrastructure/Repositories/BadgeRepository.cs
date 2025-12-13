using HabitTracker.Application.Interfaces;
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
    public class BadgeRepository : Repository<Badge>, IBadgeRepository
    {
        private readonly HabitTrackerDbContext _context;
        public BadgeRepository(HabitTrackerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<int>> GetUserEarnedBadgeIdsAsync(int userId)
        {
            // UserBadges tablosuna erişmek için context'i kullanıyoruz.
            // Sadece BadgeId'leri integer listesi olarak döndürüyoruz (Performans için).
            return await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.BadgeId)
                .ToListAsync();
        }
    }
}
