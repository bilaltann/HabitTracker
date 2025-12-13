using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces.Repositories
{
    public interface IHabitRepository:IRepository<Habit>
    {
        // kullanıcının sadece kendi (ve aktif ) alışkanlıklarını getiren metot
        Task<IEnumerable<Habit>> GetHabitsByUserIdAsync(int userId);

    }
}
