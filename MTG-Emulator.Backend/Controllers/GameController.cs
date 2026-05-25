using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.GameDTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : MtgController
    {
        private readonly MTGContext context;
        private readonly HashSet<string> gameCodes = new HashSet<string>();

        public GameController(MTGContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Authorize(Policy = "PlayerOrAdmin")]
        public async Task<ActionResult<GameResponseDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.ApiUserId == callerId);

            if (player == null)
                return NotFound("Player not found.");
            
            if (player.CurrentGameId != null)
                return Conflict(new GameResponseDto { Success = false, Message = "You are already in a game." });
            

            string code = generateCode();
            while (await context.Games.AnyAsync(g => g.GameCode == code))
                code = generateCode();

            gameCodes.Add(code);

            var game = new Game
            {
                GameCode = code,
                MaxPlayers = dto.MaxPlayers,
                CurrentPlayers = 1,
                HostName = player.Username,
                PlayerNames = new List<string> { player.Username },
                Status = "Waiting"
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            player.CurrentGameId = game.GameId;
            game.Players.Add(player);
            await context.SaveChangesAsync();

            return Ok(new GameResponseDto
            {
                Success = true,
                GameCode = code,
                MaxPlayers = game.MaxPlayers,
                CurrentPlayers = game.CurrentPlayers,
                Message = "Game created."
            });
        }

       [HttpPost("join")]
        [Authorize(Policy = "PlayerOnly")]
        public async Task<ActionResult<GameResponseDto>> JoinGame([FromBody] JoinGameDto dto)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.ApiUserId == callerId);

            if (player == null)
                return NotFound("Player not found.");

            if (player.CurrentGameId != null)
                return Conflict(new GameResponseDto { Success = false, Message = "You are already in a game." });

            var game = await context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.GameCode == dto.GameCode && g.Status == "Waiting");

            if (game == null)
                return NotFound(new GameResponseDto
                    { Success = false, Message = "Game not found or already started." });

            if (game.CurrentPlayers >= game.MaxPlayers)
                return Conflict(new GameResponseDto { Success = false, Message = "Game is full." });

            if (game.HostName == player.Username)
                return Conflict(new GameResponseDto { Success = false, Message = "You cannot join your own game." });

            game.Players.Add(player);
            player.CurrentGameId = game.GameId;
            game.PlayerNames.Add(player.Username);
            game.CurrentPlayers++;

            if (game.CurrentPlayers == game.MaxPlayers)
            {
                game.Status = "InProgress";
                game.PlayerNames = game.PlayerNames.OrderBy(n => Random.Shared.Next()).ToList();
            }

            await context.SaveChangesAsync();

            return Ok(new GameResponseDto
            {
                Success = true,
                GameCode = game.GameCode,
                MaxPlayers = game.MaxPlayers,
                CurrentPlayers = game.CurrentPlayers,
                PlayerNames = game.PlayerNames,
                CurrentPlayerName = game.PlayerNames.FirstOrDefault(),
                Message = "Joined successfully."
            });
        }

        [HttpDelete("{code}/leave")]
        [Authorize(Policy = "PlayerOnly")]
        public async Task<ActionResult> LeaveGame(string code)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var player = await context.Players
                .FirstOrDefaultAsync(p => p.ApiUserId == callerId);

            if (player == null)
                return NotFound("Player not found.");

            var game = await context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.GameCode == code);

            if (game == null)
                return NotFound("Game not found.");

            game.Players.Remove(player);
            game.PlayerNames.Remove(player.Username);
            game.CurrentPlayers--;
            player.CurrentGameId = null;

            if (game.CurrentPlayers <= 0)
                context.Games.Remove(game);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<GameResponseDto>> GetGame(string code)
        {
            var game = await context.Games
                .FirstOrDefaultAsync(g => g.GameCode == code);

            if (game == null)
                return NotFound(new GameResponseDto { Success = false, Message = "Game not found." });

            return Ok(new GameResponseDto
            {
                Success = true,
                GameCode = game.GameCode,
                MaxPlayers = game.MaxPlayers,
                CurrentPlayers = game.CurrentPlayers,
                Message = game.Status,
                PlayerNames = game.PlayerNames,
                CurrentPlayerName = game.PlayerNames.FirstOrDefault() ?? string.Empty
                
            });
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteGame(string code)
        {
            var game = await context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.GameCode == code);

            if (game == null)
                return NotFound("Game not found.");

            foreach (var player in game.Players)
                player.CurrentGameId = null;

            context.Games.Remove(game);
            await context.SaveChangesAsync();
            return NoContent();
        }

        private static string generateCode()
        {
            const int maxlength = 6;
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder(maxlength);
            var rng = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < maxlength; i++)
                sb.Append(chars[rng.Next(chars.Length)]);
            return sb.ToString();
        }
    }
}