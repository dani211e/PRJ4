using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Backend.Controllers;
using System.Text.Json;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Player>> CreateProfile(MTGContext context, string PlayerName, string Password)
        {
            if (string.IsNullOrEmpty(PlayerName) || string.IsNullOrEmpty(Password)) return BadRequest();
            return new Player()
            {
                Username = PlayerName,
                Password = Password,
                GamesWon = 0,
                GamesLost = 0,
                GamesDrawed = 0
            };
        }

        [HttpGet("{PlayerName}")]
        public async Task<ActionResult<Player>> GetProfile(MTGContext context, string PlayerName)
        {
            var Player = context.Players
                .FirstOrDefaultAsync(Player => Player.Username == PlayerName);

            if (Player == null) return NotFound();

            return Ok(Player);
        }

        [HttpDelete("{PlayerName}")]
        public async Task<ActionResult<Player>> DeleteProfile(MTGContext context, string PlayerName)
        {
            if(string.IsNullOrEmpty(PlayerName)) return BadRequest();
            Player PlayerToRemove = await context.Players.FirstOrDefaultAsync(Player => Player.Username == PlayerName);

            if(PlayerToRemove == null) return NotFound();
            context.Players.Remove(PlayerToRemove);
            await context.SaveChangesAsync();

            return Ok();

        }
    }
}
