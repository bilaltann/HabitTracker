using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly HabitTrackerDbContext _context;
        public UserRepository(HabitTrackerDbContext context) : base(context)
        {
            _context = context;
        }

        public  async Task<User> GetByEmailAsync(string email)
        {
            return  _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}
