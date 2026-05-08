using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly MTGContext context;

        public GameController(MTGContext context)
        {
            this.context = context;
        }

        // POST api/Game
        // Unity sends: { gameCode, maxPlayers, hostName }
        [HttpPost]
        public async Task<ActionResult<GameResponseDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.GameCode) || dto.MaxPlayers < 2 || dto.MaxPlayers > 4)
                return BadRequest(new GameResponseDto { success = false, message = "Invalid game settings." });

            // Reject duplicate codes (extremely unlikely but safe)
            bool codeExists = await context.Games
                .AnyAsync(g => g.GameCode == dto.GameCode && g.Status == "Waiting");

            if (codeExists)
                return Conflict(new GameResponseDto { success = false, message = "Game code already in use." });

            var game = new Game
            {
                GameCode       = dto.GameCode,
                MaxPlayers     = dto.MaxPlayers,
                CurrentPlayers = 1,
                HostName       = dto.HostName,
                Status         = "Waiting"
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            return Ok(new GameResponseDto
            {
                success        = true,
                gameCode       = game.GameCode,
                maxPlayers     = game.MaxPlayers,
                currentPlayers = game.CurrentPlayers,
                message        = "Game created."
            });
        }

        // POST api/Game/join
        // Unity sends: { gameCode, playerName }
        [HttpPost("join")]
        public async Task<ActionResult<GameResponseDto>> JoinGame([FromBody] JoinGameDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.GameCode))
                return BadRequest(new GameResponseDto { success = false, message = "Game code is required." });

            var game = await context.Games
                .FirstOrDefaultAsync(g => g.GameCode == dto.GameCode && g.Status == "Waiting");

            if (game == null)
                return NotFound(new GameResponseDto { success = false, message = "Game not found or already started." });

            if (game.CurrentPlayers >= game.MaxPlayers)
                return Conflict(new GameResponseDto { success = false, message = "Game is full." });

            game.CurrentPlayers++;

            if (game.CurrentPlayers == game.MaxPlayers)
                game.Status = "InProgress";

            await context.SaveChangesAsync();

            return Ok(new GameResponseDto
            {
                success        = true,
                gameCode       = game.GameCode,
                maxPlayers     = game.MaxPlayers,
                currentPlayers = game.CurrentPlayers,
                message        = "Joined successfully."
            });
        }

        // GET api/Game/{code}  — useful for polling lobby state
        [HttpGet("{code}")]
        public async Task<ActionResult<GameResponseDto>> GetGame(string code)
        {
            var game = await context.Games
                .FirstOrDefaultAsync(g => g.GameCode == code);

            if (game == null)
                return NotFound(new GameResponseDto { success = false, message = "Game not found." });

            return Ok(new GameResponseDto
            {
                success        = true,
                gameCode       = game.GameCode,
                maxPlayers     = game.MaxPlayers,
                currentPlayers = game.CurrentPlayers,
                message        = game.Status
            });
        }
    }
    
    // DTOs (backend side)

    public class CreateGameDto
    {
        public string GameCode   { get; set; } = string.Empty;
        public int    MaxPlayers { get; set; }
        public string HostName   { get; set; } = string.Empty;
    }

    public class JoinGameDto
    {
        public string GameCode   { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
    }

    public class GameResponseDto
    {
        public bool   success        { get; set; }
        public string gameCode       { get; set; } = string.Empty;
        public int    maxPlayers     { get; set; }
        public int    currentPlayers { get; set; }
        public string message        { get; set; } = string.Empty;
    }
}
