using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitDTOs
{
    public class CreateHabitDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int FrequencyId { get; set; } // Enum değeri (0: Daily, 1: Weekly)
        public int UserId { get; set; }
    }
}
