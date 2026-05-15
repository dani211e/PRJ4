using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.CardFaceDTO;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;

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
    public class DeckController : MtgController
    {
        private readonly MTGContext context;

        public DeckController(MTGContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult<DeckDto>> CreateDeck([FromBody] CreateDeckDto deckDto)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.ApiUserId == callerId);

            if (player == null)
                return NotFound("Player profile not found.");

            var invalidCardNames = new List<string>();
            var deckCards = new List<DeckCard>();
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
                    deckCards.Add(new DeckCard { Card = cardEntity, Quantity = amount });
                else
                    invalidCardNames.Add(name);
            }

            if (invalidCardNames.Count != 0)
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards do not exist",
                    InvalidCards = invalidCardNames,
                });

            var commanderInDeck = deckCards.Any(dc => dc.Card.Name == deckDto.Commander);
            if (!commanderInDeck)
                return BadRequest($"Commander '{deckDto.Commander}' must be included in the card list.");

            var deck = new Deck
            {
                DeckName = deckDto.DeckName,
                DeckCommander = deckDto.Commander,
                DeckCards = deckCards,
                Player = player,
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var resultDto = new DeckDto
            {
                DeckId = deck.DeckId,
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.DeckCards
                    .SelectMany(dc => Enumerable.Repeat(new CardDto
                    {
                        CardId = dc.Card.CardId,
                        Name = dc.Card.Name,
                        OracleText = dc.Card.OracleText,
                        ImageUri = dc.Card.ImageUri,
                    }, dc.Quantity))
                    .ToList(),
            };

            return CreatedAtAction(nameof(GetDeckById), new { deck.DeckId }, resultDto);
        }

        [HttpGet("player/{username}")]
        public async Task<ActionResult<List<DeckDto>>> GetAllDecksByUsername(string username)
        {
            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == username);

            if (player == null)
                return NotFound($"Player '{username}' not found.");

            if (!IsOwnerOrAdmin(player.ApiUserId))
                return Forbid();

            var decks = await context.Decks
                .Include(d => d.DeckCards)
                    .ThenInclude(dc => dc.Card)
                .Where(d => d.Player.Username == username)
                .ToListAsync();

            var deckDtos = decks.Select(deck => new AllDecksDto
            {
                DeckId = deck.DeckId,
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                DeckImageUri = deck.DeckCards
                    .FirstOrDefault(dc => dc.Card.Name == deck.DeckCommander)?.Card.ImageUri ?? string.Empty
            }).ToList();

            return Ok(deckDtos);
        }

        [HttpGet("{DeckId:int}")]
        public async Task<ActionResult<DeckDto>> GetDeckById(int deckId)
        {
            var deck = await context.Decks
                .Include(d => d.DeckCards)
                .ThenInclude(dc => dc.Card)
                .ThenInclude(c => c.AltFace)
                .Include(d => d.DeckCards)
                .ThenInclude(dc => dc.Card)
                .ThenInclude(c => c.RelatedCards)
                .Include(d => d.Player)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.DeckId == deckId);

            if (deck == null)
                return NotFound();

            if (!IsOwnerOrAdmin(deck.Player.ApiUserId))
                return Forbid();

            var deckDto = new DeckDto
            {
                DeckId = deck.DeckId,
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.DeckCards
                    .SelectMany(dc => Enumerable.Repeat(new CardDto
                    {
                        CardId = dc.Card.CardId,
                        ScryfallId = dc.Card.ScryfallId,
                        Name = dc.Card.Name,
                        OracleText = dc.Card.OracleText,
                        ImageUri = dc.Card.ImageUri,
                        AltFace = dc.Card.AltFace == null ? null : new CardFaceDto
                        {
                            Name = dc.Card.AltFace.Name,
                            OracleText = dc.Card.AltFace.OracleText,
                            ImageUri = dc.Card.AltFace.ImageUri,
                        },
                        RelatedCards = dc.Card.RelatedCards.Select(rc => new RelatedCardDto
                        {
                            RelatedCardId = rc.RelatedCardId,
                            Name = rc.Name,
                            ImageUri = rc.ImageUri,
                        }).ToList()
                    }, dc.Quantity))
                    .ToList(),
            };

            return Ok(deckDto);
        }

        [HttpDelete("{DeckId:int}")]
        public async Task<IActionResult> DeleteDeckByName(int deckid)
        {
            var deck = await context.Decks
                .Include(d => d.Player)
                .Include(d => d.DeckCards)
                .FirstOrDefaultAsync(d => d.DeckId == deckid);

            if (deck == null)
                return NotFound();

            if (!IsOwnerOrAdmin(deck.Player.ApiUserId))
                return Forbid();

            context.Decks.Remove(deck);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{deckId:int}")]
        public async Task<ActionResult<DeckDto>> UpdateDeck(int deckId, [FromBody] UpdateDeckDto? deckDto)
        {
            if (deckDto == null)
                return BadRequest();

            var deck = await context.Decks
                .Include(d => d.Player)
                .Include(d => d.DeckCards).ThenInclude(deckCard => deckCard.Card)
                .FirstOrDefaultAsync(d => d.DeckId == deckId);

            if (deck == null)
                return NotFound();

            if (!IsOwnerOrAdmin(deck.Player.ApiUserId))
                return Forbid();

            deck.DeckName = deckDto.DeckName;
            deck.DeckCommander = deckDto.Commander;

            var invalidCardNames = new List<string>();
            deck.DeckCards.Clear();

            if (!string.IsNullOrWhiteSpace(deckDto.CardList))
            {
                string[] lines = deckDto.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    if (firstSpace == -1)
                        return BadRequest($"Wrong line in card list: '{line}'");
                    if (!int.TryParse(line.Substring(0, firstSpace), out int amount))
                        return BadRequest($"Invalid quantity in line: '{line}'");

                    string name = line.Substring(firstSpace + 1);
                    var cardEntity = await context.Cards.FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        deck.DeckCards.Add(new DeckCard { Card = cardEntity, Quantity = amount });
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

            var commanderInDeck = deck.DeckCards.Any(dc => dc.Card.Name == deckDto.Commander);
            if (!commanderInDeck)
                return BadRequest($"Commander '{deckDto.Commander}' must be included in the card list.");

            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}