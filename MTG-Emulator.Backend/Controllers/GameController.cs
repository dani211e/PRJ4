using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.GameDTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly MTGContext context;
        private readonly HashSet<string> gameCodes = new HashSet<string>();

        public GameController(MTGContext context)
        {
            this.context = context;
        }

        // POST api/Game
        // Unity sends: { gameCode, maxPlayers, hostName }
        [HttpPost]
        public async Task<ActionResult<GameResponseDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            string code = generateCode();
            while (gameCodes.Contains(code))
            {
                code = generateCode();
            }

            gameCodes.Add(code);

            var game = new Game
            {
                GameCode = code,
                MaxPlayers = dto.MaxPlayers,
                CurrentPlayers = 1,
                HostName = dto.HostName,
                Status = "Waiting"
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            return Ok(new GameResponseDto
            {
                success = true,
                gameCode = code,
                maxPlayers = game.MaxPlayers,
                currentPlayers = game.CurrentPlayers,
                message = "Game created."
            });
        }

        // POST api/Game/join
        // Unity sends: { gameCode, playerName }
        [HttpPost("join")]
        public async Task<ActionResult<GameResponseDto>> JoinGame([FromBody] JoinGameDto dto)
        {
            var game = await context.Games
                .FirstAsync(g => g.GameCode == dto.GameCode && g.Status == "Waiting");

            if (game == null)
                return NotFound(new GameResponseDto
                    { success = false, message = "Game not found or already started." });

            if (game.CurrentPlayers >= game.MaxPlayers)
                return Conflict(new GameResponseDto { success = false, message = "Game is full." });

            game.CurrentPlayers++;

            if (game.CurrentPlayers == game.MaxPlayers)
                game.Status = "InProgress";

            await context.SaveChangesAsync();

            return Ok(new GameResponseDto
            {
                success = true,
                gameCode = game.GameCode,
                maxPlayers = game.MaxPlayers,
                currentPlayers = game.CurrentPlayers,
                message = "Joined successfully."
            });
        }

        // GET api/Game/{code}
        [HttpGet("{code}")]
        public async Task<ActionResult<GameResponseDto>> GetGame(string code)
        {
            var game = await context.Games
                .FirstOrDefaultAsync(g => g.GameCode == code);

            if (game == null)
                return NotFound(new GameResponseDto { success = false, message = "Game not found." });

            return Ok(new GameResponseDto
            {
                success = true,
                gameCode = game.GameCode,
                maxPlayers = game.MaxPlayers,
                currentPlayers = game.CurrentPlayers,
                message = game.Status
            });
        }

        [HttpDelete("{code}")]
        public async Task<ActionResult<GameResponseDto>> DeleteGame(string code)
        {
            var game = await context.Games
                .FirstAsync(g => g.GameCode == code);

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