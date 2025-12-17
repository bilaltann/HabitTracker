using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces.Repositories
{
    public interface IHabitLogRepository : IRepository<HabitLog>
    {
        // belirli bir tarihte , belirli bir alışkanlık yapılmış mı? (işaretleme için)
        Task<HabitLog?> GetLogByDateAsync(int habitId,DateTime date);

        // kullanıcının o gün yaptığı tüm işlemleri getirir(listeleme ekranı için)
        Task<IEnumerable<HabitLog>> GetLogsByUserIdAndDateAsync(int userId, DateTime date);

        // --- YENİ EKLENEN: Son aktiviteleri getir ---
        Task<IEnumerable<HabitLog>> GetRecentLogsWithDetailsAsync(int count);
    }
}
