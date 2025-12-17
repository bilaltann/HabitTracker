using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.AdminDTOs
{
    public class AdminActivityDto
    {
        public string Title { get; set; }       // Örn: "Ahmet bir alışkanlık tamamladı"
        public string Description { get; set; } // Örn: "Kitap Okuma (Kişisel Gelişim)"
        public DateTime Date { get; set; }      // İşlem zamanı
        public string Type { get; set; }        // "success" (Yeşil ikon için)
    }
}
