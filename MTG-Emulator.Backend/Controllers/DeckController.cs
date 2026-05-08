using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;

namespace MTG_Emulator.Backend.Controllers
{
    public class InvalidCardsResponse
    {
        public string Error { get; set; } = string.Empty;
        public List<string> InvalidCards { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PlayerOrAdmin")]
    public class DeckController : BaseController
    {
        private readonly MTGContext context;

        public DeckController(MTGContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult<DeckDto>> CreateDeck([FromBody] CreateDeckDto deckDto)
        {
            if (string.IsNullOrWhiteSpace(deckDto.DeckName))
                return BadRequest("Invalid deck name");
            if (string.IsNullOrWhiteSpace(deckDto.CardList))
                return BadRequest("Invalid card data");

            // Resolve player from JWT instead of trusting the request body
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.ApiUserId == callerId);

            if (player == null)
                return NotFound("Player profile not found.");

            // Map cards from names
            var cards = new List<Card>();
            var invalidCardNames = new List<string>();
            string[] lines = deckDto.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                int firstSpace = line.IndexOf(' ');
                if (firstSpace == -1)
                    return BadRequest($"Wrong line in card list: '{line}'");
                if (!int.TryParse(line.Substring(0, firstSpace), out int amount))
                    return BadRequest($"Invalid quantity in line: '{line}'");

                string name = line.Substring(firstSpace + 1);
                var cardEntity = await context.Cards
                    .FirstOrDefaultAsync(c => c.Name == name);

                if (cardEntity != null)
                    for (int i = 0; i < amount; i++)
                        cards.Add(cardEntity);
                else
                    invalidCardNames.Add(name);
            }

            if (invalidCardNames.Count != 0)
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards do not exist",
                    InvalidCards = invalidCardNames,
                });

            var deck = new Deck
            {
                DeckName = deckDto.DeckName,
                DeckCommander = deckDto.Commander,
                Cards = cards,
                Player = player,
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var resultDto = new DeckDto
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards.Select(c => new CardDto
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    OracleText = c.OracleText,
                    ImageUri = c.ImageUri,
                }).ToList(),
            };

            return CreatedAtAction(nameof(GetDeckByName), new { deck.DeckName }, resultDto);
        }

        [HttpGet("player/{playerId}")]
        public async Task<ActionResult<List<DeckDto>>> GetAllDecksByPlayerId(int playerId)
        {
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player == null)
                return NotFound($"Player '{playerId}' not found.");

            var decks = await context.Decks
                .Include(d => d.Cards)
                .Where(d => d.Player.PlayerId == playerId)
                .ToListAsync();

            var deckDtos = decks.Select(deck => new DeckDto
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards.Select(c => new CardDto
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    OracleText = c.OracleText,
                    ImageUri = c.ImageUri,
                }).ToList(),
            }).ToList();

            return Ok(deckDtos);
        }

        [HttpGet("{DeckName}")]
        public async Task<ActionResult<DeckDto>> GetDeckByName(string deckName)
        {
            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null)
                return NotFound();

            var deckDto = new DeckDto
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards
                    .Select(c => new CardDto
                    {
                        CardId = c.CardId,
                        Name = c.Name,
                        OracleText = c.OracleText,
                        ImageUri = c.ImageUri,
                    })
                    .ToList(),
            };

            return Ok(deckDto);
        }

        [HttpDelete("{DeckName}")]
        public async Task<IActionResult> DeleteDeckByName(string deckName)
        {
            if (string.IsNullOrWhiteSpace(deckName))
                return BadRequest();

            var deck = await context.Decks
                .Include(d => d.Player)
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null)
                return NotFound();

            if (!IsOwnerOrAdmin(deck.Player.ApiUserId))
                return Forbid();

            context.Decks.Remove(deck);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{DeckName}")]
        public async Task<ActionResult<DeckDto>> UpdateDeck(string deckName, [FromBody] UpdateDeckDto? deckDto)
        {
            if (string.IsNullOrWhiteSpace(deckName) || deckDto == null)
                return BadRequest();

            var deck = await context.Decks
                .Include(d => d.Player)
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null)
                return NotFound();

            if (!IsOwnerOrAdmin(deck.Player.ApiUserId))
                return Forbid();

            deck.DeckName = deckDto.DeckName;
            deck.DeckCommander = deckDto.Commander;

            var invalidCardNames = new List<string>();
            deck.Cards.Clear();
            if (!string.IsNullOrWhiteSpace(deckDto.CardList))
            {
                string[] lines = deckDto.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    if (firstSpace == -1)
                        return BadRequest($"Wrong line in card list: '{line}'");
                    if (!int.TryParse(line.Substring(0, firstSpace), out int num))
                        return BadRequest($"Invalid quantity in line: '{line}'");

                    string name = line.Substring(firstSpace + 1);
                    var cardEntity = await context.Cards.FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        for (int i = 0; i < num; i++)
                            deck.Cards.Add(cardEntity);
                    else
                        invalidCardNames.Add(name);
                }
            }

            if (invalidCardNames.Any())
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards do not exist",
                    InvalidCards = invalidCardNames,
                });

            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}