using HabitTracker.Application.DTOs.FriendshipDTOs;
using HabitTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost("send-request")]
        public async Task<IActionResult> SendRequest([FromBody] SendFriendRequestDto dto)
        {
            try
            {
                await _friendService.SendRequestAsync(GetUserId(), dto.TargetEmail);
                return Ok(new { message = "İstek gönderildi ve kullanıcıya mail atıldı." });
            }
            catch (Exception ex)
            {
                // ESKİSİ: return BadRequest(ex.Message); -> Bu düz yazı döner, frontend'i bozar.

                // YENİSİ: JSON Objesi döner -> Frontend bunu sever.
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondFriendRequestDto dto)
        {
            await _friendService.RespondToRequestAsync(GetUserId(), dto.RequestId, dto.IsAccepted);
            return Ok(new { message = dto.IsAccepted ? "Arkadaşlık kabul edildi." : "İstek reddedildi." });
        }

        [HttpGet("received")]
        public async Task<IActionResult> GetReceived()
        {
            var result = await _friendService.GetReceivedRequestsAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("sent")]
        public async Task<IActionResult> GetSent()
        {
            var result = await _friendService.GetSentRequestsAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetFriends()
        {
            var result = await _friendService.GetActiveFriendsAsync(GetUserId());
            return Ok(result);
        }
    }
}
