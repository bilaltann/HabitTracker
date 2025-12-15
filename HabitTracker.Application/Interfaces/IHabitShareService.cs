using HabitTracker.Application.DTOs.HabitShareDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IHabitShareService
    {
        Task SendInvitationAsync(int senderId, SendHabitInvitationDto dto);
        Task RespondToInvitationAsync(int receiverId, RespondHabitInvitationDto dto);
        Task<IEnumerable<HabitInvitationListDto>> GetPendingInvitationsAsync(int userId);
    }
}
