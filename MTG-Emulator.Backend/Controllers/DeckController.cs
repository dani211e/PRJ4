using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    public class InvalidCardsResponse
    {
        public string Error { get; set; } = string.Empty;
        public List<string> InvalidCards { get; set; } = new List<string>();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DeckController : ControllerBase
    {
        private readonly MTGContext context;

        public DeckController(MTGContext context)
        {
            this.context = context;
        }

        // POST: api/Deck
        [HttpPost]
        public async Task<ActionResult<DeckDto>> CreateDeck([FromBody] CreateDeckDto deckDto)
        {
            if (string.IsNullOrWhiteSpace(deckDto.DeckName) ||
                string.IsNullOrWhiteSpace(deckDto.PlayerName) ||
                string.IsNullOrWhiteSpace(deckDto.CardList))
            {
                return BadRequest("Invalid deck data");
            }

            // Map cards from names
            var cards = new List<Card>();
            var invalidCardnames = new List<string>();
            if (!string.IsNullOrWhiteSpace(deckDto.CardList))
            {
                string[] lines = deckDto.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    if (firstSpace == -1) return BadRequest(new { error = $"Wrong line in card list: '{line}'" });
                    if (!int.TryParse(line.Substring(0, firstSpace), out int num)) return BadRequest(new { error = $"Invalid quantity in line: '{line}'" });
                    int amount = int.Parse(line.Substring(0, firstSpace));
                    string name = line.Substring(firstSpace + 1);

                    var cardEntity = await context.Cards
                        .FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                    {
                        for (int i = 0; i < amount; i++)
                            cards.Add(cardEntity);
                    }
                    else
                    {
                        invalidCardnames.Add(name);
                    }
                }
            }

            if (invalidCardnames.Any())
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards does not exist",
                    InvalidCards = invalidCardnames
                });

            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == deckDto.PlayerName);

            if (player == null)
                return BadRequest(new { error = $"Player '{deckDto.PlayerName}' not found." });

            var deck = new Deck
            {
                DeckName = deckDto.DeckName,
                DeckCommander = deckDto.Commander,
                Cards = cards,
                Player = player
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            // Map to DTO for return
            var resultDto = new DeckDto
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards.Select(c => new CardDto
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    OracleText = c.OracleText,
                    ImageUri = c.ImageUri
                }).ToList()
            };

            return CreatedAtAction(nameof(GetDeckByName), new { deck.DeckName }, resultDto);
        }

        // GET: api/Deck/{DeckName}
        [HttpGet("{DeckName}")]
        public async Task<ActionResult<DeckDto>> GetDeckByName(string deckName)
        {
            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            var deckDto = new DeckDto
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards?
                    .Select(c => new CardDto
                    {
                        CardId = c.CardId,
                        Name = c.Name,
                        OracleText = c.OracleText,
                        ImageUri = c.ImageUri
                    })
                    .ToList() ?? new List<CardDto>()
            };

            return Ok(deckDto);
        }

        // DELETE: api/Deck/{DeckName}
        [HttpDelete("{DeckName}")]
        public async Task<IActionResult> DeleteDeckByName(string deckName)
        {
            if (string.IsNullOrWhiteSpace(deckName)) return BadRequest();

            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            context.Decks.Remove(deck);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Deck/{DeckName}
        [HttpPut("{DeckName}")]
        public async Task<ActionResult<DeckDto>> UpdateDeck(string deckName, [FromBody] CreateDeckDto deckDto)
        {
            if (string.IsNullOrWhiteSpace(deckName) || deckDto == null) return BadRequest();

            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            // Update deck properties
            deck.DeckName = deckDto.DeckName;
            deck.DeckCommander = deckDto.Commander;

            // Update cards
            deck.Cards.Clear();
            if (!string.IsNullOrWhiteSpace(deckDto.CardList))
            {
                string[] lines = deckDto.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    int num = int.Parse(line.Substring(0, firstSpace));
                    string name = line.Substring(firstSpace + 1);

                    var cardEntity = await context.Cards
                        .FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        for (int i = 0; i < num; i++)
                            deck.Cards.Add(cardEntity);
                }
            }

            await context.SaveChangesAsync();

            // Return updated DTO
            var resultDto = new DeckDto
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards.Select(c => new CardDto
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    OracleText = c.OracleText,
                    ImageUri = c.ImageUri
                }).ToList()
            };

            return Ok(resultDto);
        }
    }
}
