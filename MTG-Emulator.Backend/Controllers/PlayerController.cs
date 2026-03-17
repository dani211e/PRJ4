using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        public enum GameResults
        {
            Win = 1,
            Draw = 0,
            Loss = -1
        }

        private readonly MTGContext _context;

        public PlayerController(MTGContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Player>> CreateProfile(string playerName, string password)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password))
                return BadRequest("Username and password are required.");

            // Check if player already exists
            var existingPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);
            if (existingPlayer != null)
                return Conflict("Player with this username already exists.");

            var player = new Player
            {
                Username = playerName,
                Password = password,
                GamesWon = 0,
                GamesLost = 0,
                GamesDrawed = 0
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfile), new { PlayerName = player.Username }, player);
        }

        [HttpGet("{PlayerName}")]
        public async Task<ActionResult<PlayerDto>> GetProfile(string playerName)
        {
            if(string.IsNullOrEmpty(playerName)) return BadRequest();
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null) return NotFound();

            var dto = new PlayerDto
            {
                Username = player.Username,
                GamesWon = player.GamesWon,
                GamesLost = player.GamesLost,
                GamesDrawed = player.GamesDrawed,
            };

            return Ok(dto);
        }

        [HttpDelete("{PlayerName}")]
        public async Task<ActionResult> DeleteProfile(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName)) return BadRequest();

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null) return NotFound();

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Update player's game stats
        [HttpPut("{PlayerName}")]
        public async Task<ActionResult<Player>> UpdatePlayerStats(string playerName, GameResults result)
        {
            if (string.IsNullOrEmpty(playerName)) return NotFound();

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null) return NotFound();

            switch (result)
            {
                case GameResults.Win:
                    player.GamesWon++;
                    break;
                case GameResults.Draw:
                    player.GamesDrawed++;
                    break;
                case GameResults.Loss:
                    player.GamesLost++;
                    break;
                default:
                    return BadRequest("Invalid game result.");
            }

            await _context.SaveChangesAsync();
            return Ok(player);
        }

        // Reset player password
        [HttpPut("Profile/{PlayerName}")]
        public async Task<ActionResult<Player>> ResetPlayerPassword(string playerName, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return BadRequest("Password cannot be empty.");

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null) return NotFound();

            player.Password = password;
            await _context.SaveChangesAsync();

            return Ok(player);
        }

    }
}
