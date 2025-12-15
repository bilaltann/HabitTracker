using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitShareDTOs
{
    public class HabitInvitationListDto
    {
        public int InvitationId { get; set; }
        public string SenderName { get; set; }
        public string HabitName { get; set; }
        public string Category { get; set; }

    }
}
