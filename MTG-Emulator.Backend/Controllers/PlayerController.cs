using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO.PlayerDTO;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly MTGContext context;

        public PlayerController(MTGContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Player>> CreateProfile(string playerName, string password)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password))
                return BadRequest("Username and password are required.");

            // Check if player already exists
            var existingPlayer = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);
            if (existingPlayer != null)
                return Conflict("Player with this username already exists.");

            var player = new Player
            {
                Username = playerName,
                Password = password,
                GamesWon = 0,
                GamesLost = 0,
                GamesDrawn = 0,
            };

            context.Players.Add(player);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfile), new { PlayerName = player.Username }, player);
        }

        [HttpGet("{PlayerName}")]
        public async Task<ActionResult<PlayerDto>> GetProfile(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                return BadRequest();
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null)
                return NotFound();

            var dto = new PlayerDto
            {
                Username = player.Username,
                GamesWon = player.GamesWon,
                GamesLost = player.GamesLost,
                GamesDrawed = player.GamesDrawn,
            };

            return Ok(dto);
        }

        [HttpDelete("{PlayerName}")]
        public async Task<ActionResult> DeleteProfile(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return BadRequest();

            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null)
                return NotFound();

            context.Players.Remove(player);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // Update player's game stats
        [HttpPut("{PlayerName}")]
        public async Task<ActionResult<Player>> UpdatePlayerStats(string playerName, GameResults result)
        {
            if (string.IsNullOrEmpty(playerName))
                return NotFound();

            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null)
                return NotFound();

            switch (result)
            {
                case GameResults.Win:
                    player.GamesWon++;
                    break;
                case GameResults.Draw:
                    player.GamesDrawn++;
                    break;
                case GameResults.Loss:
                    player.GamesLost++;
                    break;
                default:
                    return BadRequest("Invalid game result.");
            }

            await context.SaveChangesAsync();
            return NoContent();
        }

        // Reset player password
        [HttpPut("Profile/{PlayerName}")]
        public async Task<ActionResult<Player>> ResetPlayerPassword(string playerName, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return BadRequest("Password cannot be empty.");

            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null)
                return NotFound();

            player.Password = password;
            await context.SaveChangesAsync();

            return NoContent();
        }
    }

    public enum GameResults
    {
        Win = 1,
        Draw = 0,
        Loss = -1,
    }
}
