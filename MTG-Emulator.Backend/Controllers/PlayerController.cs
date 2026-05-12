using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PlayerOrAdmin")]
    public class PlayerController : MtgController
    {
        private readonly MTGContext context;

        public PlayerController(MTGContext context)
        {
            this.context = context;
        }

        [HttpGet("{PlayerName}")]
        public async Task<ActionResult<PlayerDto>> GetProfile(string playerName)
        {
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
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null)
                return NotFound();
            
            if (!IsOwnerOrAdmin(player.ApiUserId)) 
                return Forbid();

            context.Players.Remove(player);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // Update player's game stats
        [HttpPut("{PlayerName}")]
        public async Task<ActionResult<Player>> UpdatePlayerStats(string playerName, GameResults result)
        {
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == playerName);

            if (player == null)
                return NotFound();
            
            if (!IsOwnerOrAdmin(player.ApiUserId)) 
                return Forbid();

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
    }

    public enum GameResults
    {
        Win = 1,
        Draw = 0,
        Loss = -1,
    }
}
