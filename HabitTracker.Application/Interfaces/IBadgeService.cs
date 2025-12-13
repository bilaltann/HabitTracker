using HabitTracker.Application.DTOs.BadgeDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IBadgeService
    {
        // GÜNCELLEME: İkinci parametre olarak 'string imagePath' ekledik
        Task<BadgeDTO> CreateBadgeAsync(CreateBadgeDto createDto, string imagePath);

        Task<IEnumerable<BadgeDTO>> GetAllBadgesAsync();

        Task<IEnumerable<int>> GetEarnedBadgeIdsAsync(int userId);

        Task CheckAndAwardBadgesAsync(int userId);


    }
}
