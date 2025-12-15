using HabitTracker.Application.DTOs.HabitShareDTOs;
using HabitTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // bu controller'a sadece token' olanlar erişebilir..
    public class HabitShareController : ControllerBase
    {
        private readonly IHabitShareService _habitShareService;

        public HabitShareController(IHabitShareService habitShareService)
        {
           _habitShareService = habitShareService;
        }

        // Token'dan ID alma
        private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendHabitInvitationDto dto)
        {
            await _habitShareService.SendInvitationAsync(GetUserId(), dto);
            return Ok(new { message = "Davet gönderildi." });
        }

        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondHabitInvitationDto dto)
        {
            await _habitShareService.RespondToInvitationAsync(GetUserId(), dto);
            return Ok(new { message = dto.IsAccepted ? "Alışkanlık eklendi!" : "Reddedildi." });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _habitShareService.GetPendingInvitationsAsync(GetUserId());
            return Ok(result);
        }
    }
}
