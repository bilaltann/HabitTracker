using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitDTOs
{
    public class ToggleHabitRequestDTO
    {
        public int HabitId { get; set; }
        public DateTime Date { get; set; }
    }
}
