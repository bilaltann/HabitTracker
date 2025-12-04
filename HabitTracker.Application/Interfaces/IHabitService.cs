using HabitTracker.Application.DTOs.HabitDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IHabitService
    {
            // Listeleme
            Task<IEnumerable<HabitDto>> GetAllHabitsByUserIdAsync(int userId);

            // Tekil Getirme (Detay için)
            Task<HabitDto> GetHabitByIdAsync(int habitId);

            // Ekleme
            Task<HabitDto> CreateHabitAsync(CreateHabitDto createDto);

            // Güncelleme
            Task UpdateHabitAsync(UpdateHabitDto updateDto);

            // Silme (Soft Delete)
            Task DeleteHabitAsync(int habitId);

            // --- KRİTİK METOT ---
            // "Bugün yapıldı" işaretleme veya geri alma
            Task ToggleHabitCompletionAsync(int habitId, DateTime date);
     
    }
}
