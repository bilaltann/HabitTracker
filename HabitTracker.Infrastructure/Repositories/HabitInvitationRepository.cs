using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using HabitTracker.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure.Repositories
{
    public class HabitInvitationRepository : Repository<HabitInvitation>, IHabitInvitationRepository
    {
        private readonly HabitTrackerDbContext _context;

        public HabitInvitationRepository(HabitTrackerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HabitInvitation>> GetPendingInvitationsAsync(int userId)
        {
            return await _context.HabitInvitations
           .Include(x => x.Sender) // Kim gönderdi?
           .Include(x => x.Habit)  // Hangi alışkanlık?
           .Where(x => x.ReceiverId == userId && x.Status == FriendRequestStatus.Pending)
           .ToListAsync();
        }
    }
}
