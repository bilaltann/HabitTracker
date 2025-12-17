using HabitTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class SystemLog:BaseEntity
    {
        public string Level { get; set; } // Error, Warning, Info
        public string Message { get; set; } // Hata mesajı
        public string? StackTrace { get; set; } // Hatanın teknik detayı (Hangi satırda vs.)
        public DateTime Timestamp { get; set; } = DateTime.Now; // Ne zaman oldu?
    }
}
