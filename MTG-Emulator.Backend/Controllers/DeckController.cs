using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeckController : ControllerBase
    {
        private readonly MTGContext _context;

        public DeckController(MTGContext context)
        {
            _context = context;
        }

        // POST: api/Deck
        [HttpPost]
        public async Task<ActionResult<DeckDto>> CreateDeck([FromBody] CreateDeckDto deckDto)
        {
            if (deckDto == null) return BadRequest();

            // Map cards from names
            var cards = new List<Card>();
            if (!string.IsNullOrWhiteSpace(deckDto.CardList))
            {
                string[] lines = deckDto.CardList.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int firstSpace = line.IndexOf(' ');
                    int num = int.Parse(line.Substring(0, firstSpace));
                    string name = line.Substring(firstSpace + 1);

                    var cardEntity = await _context.Cards
                        .FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        for (int i = 0; i < num; i++)
                            cards.Add(cardEntity);
                }
            }

            var player = await _context.Players
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

            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

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
                    ImageUri = c.ImageURI
                }).ToList()
            };

            return CreatedAtAction(nameof(GetDeckByName), new { deck.DeckName }, resultDto);
        }

        // GET: api/Deck/{DeckName}
        [HttpGet("{DeckName}")]
        public async Task<ActionResult<DeckDto>> GetDeckByName(string deckName)
        {
            var deck = await _context.Decks
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
                        ImageUri = c.ImageURI
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

            var deck = await _context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == deckName);

            if (deck == null) return NotFound();

            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Deck/{DeckName}
        [HttpPut("{DeckName}")]
        public async Task<ActionResult<DeckDto>> UpdateDeck(string deckName, [FromBody] CreateDeckDto deckDto)
        {
            if (string.IsNullOrWhiteSpace(deckName) || deckDto == null) return BadRequest();

            var deck = await _context.Decks
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

                    var cardEntity = await _context.Cards
                        .FirstOrDefaultAsync(c => c.Name == name);

                    if (cardEntity != null)
                        for (int i = 0; i < num; i++)
                            deck.Cards.Add(cardEntity);
                }
            }

            await _context.SaveChangesAsync();

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
                    ImageUri = c.ImageURI
                }).ToList()
            };

            return Ok(resultDto);
        }
    }
}
