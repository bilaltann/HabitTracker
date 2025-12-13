using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitDTOs
{
    public class UpdateHabitDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int FrequencyId { get; set; }
        public bool IsActive { get; set; } // Alışkanlığı askıya alma/açma

        public DateTime ExpirationDate { get; set; }

    }
}
