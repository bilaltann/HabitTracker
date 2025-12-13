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
    public class HabitRepository : Repository<Habit>, IHabitRepository
    {
        private readonly HabitTrackerDbContext _context;
        public HabitRepository(HabitTrackerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Habit>> GetHabitsByUserIdAsync(int userId)
        {
           
            return await _context.Habits.Where(h=>h.UserId == userId && h.IsActive==true)
                .ToListAsync();
        }
    }
}
