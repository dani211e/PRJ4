using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
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

            var mainResult = await parseCardList(deckDto.CardList);
            if (mainResult.FormatError != null)
                return BadRequest(mainResult.FormatError);

            var commandResult = await parseCommandZone(deckDto.CommandZone);

            var allInvalid = mainResult.InvalidNames.Concat(commandResult.InvalidNames).ToList();
            if (allInvalid.Count != 0)
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards do not exist",
                    InvalidCards = allInvalid,
                });

            var deck = new Deck
            {
                DeckName = deckDto.DeckName,
                DeckCards = mainResult.Cards,
                CommandZone = commandResult.Cards,
                Player = player,
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            return await GetDeckById(deck.DeckId);
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
                .Include(d => d.CommandZone)
                .Where(d => d.Player.Username == username)
                .ToListAsync();

            var deckDtos = decks.Select(deck => new AllDecksDto
            {
                DeckId = deck.DeckId,
                DeckName = deck.DeckName,
                DeckImageUri = deck.CommandZone.FirstOrDefault()?.ImageUri ?? string.Empty,
            }).ToList();

            return Ok(deckDtos);
        }

        [HttpGet("{DeckId:int}")]
        public async Task<ActionResult<DeckDto>> GetDeckById(int deckId)
        {
            var deck = await context.Decks
                .Include(d => d.CommandZone)
                .ThenInclude(c => c.AltFace)
                .Include(d => d.CommandZone)
                .ThenInclude(c => c.RelatedCards)
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
                CommandZone = deck.CommandZone.Select(ToCardDto).ToList(),
                Cards = deck.DeckCards
                    .SelectMany(dc => Enumerable.Repeat(ToCardDto(dc.Card), dc.Quantity))
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
                .Include(d => d.CommandZone)
                .Include(d => d.DeckCards).ThenInclude(dc => dc.Card)
                .FirstOrDefaultAsync(d => d.DeckId == deckId);

            if (deck == null)
                return NotFound();

            if (!IsOwnerOrAdmin(deck.Player.ApiUserId))
                return Forbid();

            var mainResult = await parseCardList(deckDto.CardList);
            if (mainResult.FormatError != null)
                return BadRequest(mainResult.FormatError);

            var commandResult = await parseCommandZone(deckDto.CommandZone);

            var allInvalid = mainResult.InvalidNames.Concat(commandResult.InvalidNames).ToList();
            if (allInvalid.Count != 0)
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards do not exist",
                    InvalidCards = allInvalid,
                });

            deck.DeckName   = deckDto.DeckName;
            deck.DeckCards  = mainResult.Cards;
            deck.CommandZone = commandResult.Cards;

            await context.SaveChangesAsync();
            return NoContent();
        }

        // Helpers

        private record ParseCardListResult(
            List<DeckCard> Cards,
            List<string> InvalidNames,
            string? FormatError
        );

        private record ParseCommandZoneResult(
            List<Card> Cards,
            List<string> InvalidNames
        );

        private async Task<ParseCardListResult> parseCardList(string cardList)
        {
            var cards = new List<DeckCard>();
            var invalidNames = new List<string>();

            string[] lines = cardList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                int firstSpace = line.IndexOf(' ');
                if (firstSpace == -1)
                    return new ParseCardListResult([], [], FormatError: $"Wrong line in card list: '{line}'");
                if (!int.TryParse(line[..firstSpace], out int amount))
                    return new ParseCardListResult([], [], FormatError: $"Invalid quantity in line: '{line}'");

                string name = normalizeCardName(line[(firstSpace + 1)..].Trim());
                var    cardEntity = await context.Cards.FirstOrDefaultAsync(c => c.Name == name);

                if (cardEntity != null)
                    cards.Add(new DeckCard { Card = cardEntity, Quantity = amount });
                else
                    invalidNames.Add(name);
            }

            return new ParseCardListResult(cards, invalidNames, FormatError: null);
        }

        private async Task<ParseCommandZoneResult> parseCommandZone(List<string> commandZone)
        {
            var cards = new List<Card>();
            var invalidNames = new List<string>();

            foreach (string line in commandZone)
            {
                string name = normalizeCardName(line.Trim());
                var cardEntity = await context.Cards.FirstOrDefaultAsync(c => c.Name == name);

                if (cardEntity != null)
                    cards.Add(cardEntity);
                else
                    invalidNames.Add(name);
            }

            return new ParseCommandZoneResult(cards, invalidNames);
        }
        private static string normalizeCardName(string name)
        {
            int slashIndex = name.IndexOf(" // ", StringComparison.Ordinal);
            return slashIndex != -1 ? name[..slashIndex] : name;
        }
    }
}