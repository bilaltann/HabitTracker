using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitShareDTOs
{
    public class RespondHabitInvitationDto
    {

        public int InvitationId { get; set; }
        public bool IsAccepted { get; set; }
    }
}
