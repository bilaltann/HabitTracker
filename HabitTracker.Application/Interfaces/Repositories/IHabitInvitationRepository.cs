using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces.Repositories
{
    public interface IHabitInvitationRepository :IRepository<HabitInvitation>
    {
        // Bir kullanıcının bekleyen alışkanlık davetlerini getir (Sender ve Habit detaylarıyla)
        Task<IEnumerable<HabitInvitation>> GetPendingInvitationsAsync(int userId);
    }
}
