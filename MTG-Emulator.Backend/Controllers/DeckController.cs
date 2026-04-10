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
        public List<string> InvalidCards { get; set; } = new();
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

        [HttpPost]
        public async Task<ActionResult<DeckDTO>> CreateDeck([FromBody] CreateDeckDTO deckDTO)
        {
            // Map cards from names
            var cards = new List<Card>();
            var invalidCardnames = new List<string>();
            if (!string.IsNullOrWhiteSpace(deckDTO.CardList))
            {
                string[] lines = deckDTO.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    if (firstSpace == -1) return BadRequest(new { error = $"Wrong line in card list: '{line}'" });
                    if (!int.TryParse(line.Substring(0, firstSpace), out int num))
                        return BadRequest(new { error = $"Invalid quantity in line: '{line}'" });
                    int amount = int.Parse(line.Substring(0, firstSpace));
                    string name = line.Substring(firstSpace + 1);

                    var cardEntity = await context.Cards
                        .FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        for (int i = 0; i < amount; i++)
                            cards.Add(cardEntity);
                    else
                        invalidCardnames.Add(name);
                }
            }

            if (invalidCardnames.Any())
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards does not exist",
                    InvalidCards = invalidCardnames
                });

            var player = await context.Players
                .FirstOrDefaultAsync(p => p.Username == deckDTO.PlayerName);

            if (player == null)
                return BadRequest(new { error = $"Player '{deckDTO.PlayerName}' not found." });

            var deck = new Deck
            {
                DeckName = deckDTO.DeckName,
                DeckCommander = deckDTO.Commander,
                Cards = cards,
                Player = player
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            // Map to DTO for return
            var resultDTO = new DeckDTO
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards.Select(c => new CardDTO
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    OracleText = c.OracleText,
                    ImageUri = c.ImageUri
                }).ToList()
            };

            return CreatedAtAction(nameof(GetDeckByName), new { deck.DeckName }, resultDTO);
        }

        [HttpGet("{DeckName}")]
        public async Task<ActionResult<DeckDTO>> GetDeckByName(string deckName)
        {
            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            var deckDto = new DeckDTO
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards?
                    .Select(c => new CardDTO
                    {
                        CardId = c.CardId,
                        Name = c.Name,
                        OracleText = c.OracleText,
                        ImageUri = c.ImageUri
                    })
                    .ToList() ?? new List<CardDTO>()
            };

            return Ok(deckDto);
        }

        [HttpDelete("{DeckName}")]
        public async Task<IActionResult> DeleteDeckByName(string deckName)
        {
            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            context.Decks.Remove(deck);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{DeckName}")]
        public async Task<ActionResult<DeckDTO>> UpdateDeck(string deckName, [FromBody] UpdateDeckDTO deckDTO)
        {
            var deck = await context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            // Update deck properties
            deck.DeckName = deckDTO.DeckName;
            deck.DeckCommander = deckDTO.Commander;

            // Handle cards
            var invalidCardnames = new List<string>();
            deck.Cards.Clear();
            if (!string.IsNullOrWhiteSpace(deckDTO.CardList))
            {
                string[] lines = deckDTO.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    if (firstSpace == -1) return BadRequest(new { error = $"Wrong line in card list: '{line}'" });
                    if (!int.TryParse(line.Substring(0, firstSpace), out int num))
                        return BadRequest(new { error = $"Invalid quantity in line: '{line}'" });

                    string name = line.Substring(firstSpace + 1);
                    var cardEntity = await context.Cards.FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        for (int i = 0; i < num; i++)
                            deck.Cards.Add(cardEntity);
                    else
                        invalidCardnames.Add(name);
                }
            }

            if (invalidCardnames.Any())
                return BadRequest(new InvalidCardsResponse
                {
                    Error = "The following cards do not exist",
                    InvalidCards = invalidCardnames
                });

            await context.SaveChangesAsync();

            var resultDTO = new DeckDTO
            {
                DeckName = deck.DeckName,
                DeckCommander = deck.DeckCommander,
                Cards = deck.Cards.Select(c => new CardDTO
                {
                    CardId = c.CardId,
                    Name = c.Name,
                    OracleText = c.OracleText,
                    ImageUri = c.ImageUri
                }).ToList()
            };

            return Ok(resultDTO);
        }
    }
}
