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
        private readonly MTGContext _context;
        public PlayerController(MTGContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Player>> CreateProfile(string email, string playerName, string password)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password)) return BadRequest();
            return new Player()
            {
                Username = playerName,
                Password = password,
                GamesWon = 0,
                GamesLost = 0,
                GamesDrawed = 0
            };
        }

        [HttpGet("{PlayerName}")]
        public async Task<ActionResult<Player>> GetProfile(string playerName)
        {
            var player = _context.Players
                .FirstOrDefaultAsync(player => player.Username == playerName);

            if (player == null) return NotFound();

            return Ok(player);
        }

        [HttpDelete("{PlayerName}")]
        public async Task<ActionResult<Player>> DeleteProfile(string playerName)
        {
            if(string.IsNullOrEmpty(playerName)) return BadRequest();
            Player playerToRemove = await _context.Players.FirstOrDefaultAsync(player => player.Username == playerName);

            if(playerToRemove == null) return NotFound();
            _context.Players.Remove(playerToRemove);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public enum GameResults
        {
            Win = 1,
            Draw = 0,
            Loss = -1
        }

        [HttpPut("{PlayerName}")]
        public async Task<ActionResult<Player>> UpdatePlayerStats(string playerName, GameResults result)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(player => player.Username == playerName);
            if(player == null) return NotFound();

            switch (result)
            {
                case GameResults.Win:
                    player.GamesWon += 1;
                    await _context.SaveChangesAsync();
                    return Ok(player);
                case GameResults.Draw:
                    player.GamesDrawed += 1;
                    await _context.SaveChangesAsync();
                    return Ok(player);
                case GameResults.Loss:
                    player.GamesLost += 1;
                    await _context.SaveChangesAsync();
                    return Ok(player);
                default:
                    return BadRequest();
            }
        }

        [HttpPut("Profile/{PlayerName}")]
        public async Task<ActionResult<Player>> ResetPlayerPassword(string playerName, string password)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(player => player.Username == playerName);
            if(player == null) NotFound();

            if (playerName == player.Username)
            {
                player.Password = password;
                await _context.SaveChangesAsync();
                return Ok(player);
            }
            return BadRequest();
        }
    }
}
