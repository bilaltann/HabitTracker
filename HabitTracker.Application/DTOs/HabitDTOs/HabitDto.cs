using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitDTOs
{
    public class HabitDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Frequency { get; set; } // "Daily", "Weekly"
        public bool IsActive { get; set; }    // "Askıya alındı mı?"

        // Hesaplanan alan (Veritabanında yok, Loglardan hesaplanacak)
        public bool IsCompletedToday { get; set; }
        public int CurrentStreak { get; set; } // Zincir

        public DateTime ExpirationDate { get; set; }

    }
}
