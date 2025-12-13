using HabitTracker.Domain.Common;
using HabitTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class Habit :BaseEntity
    {
        public string Name { get; set; } // "Su İçmek"
        public string Category { get; set; }   
        public Frequency Frequency { get; set; } // Enum: Daily, Weekly
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<HabitLog> Logs { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime ExpirationDate { get; set; }
    }
}
