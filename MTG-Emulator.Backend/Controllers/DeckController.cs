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
    public class DeckController : ControllerBase
    {
        private readonly MTGContext _context;
        public DeckController(MTGContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<CreateDeckDTO>> CreateDeck([FromBody] JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Null) return BadRequest();
            var deckInfo = JsonSerializer.Deserialize<CreateDeckDTO[]>(json.GetRawText());

            string playerName = deckInfo[0].PlayerName;
            string deckName = deckInfo[1].DeckName;
            string commander =  deckInfo[2].Commander;

            return new CreateDeckDTO()
            {
                PlayerName = playerName,
                DeckName = deckName,
                Commander = commander,
                Cards = await ListOfCardsAsync(json),
            };
        }

        public async Task<List<Card>> ListOfCardsAsync(JsonElement json)
        {
            var deckInfo = JsonSerializer.Deserialize<CreateDeckDTO[]>(json.GetRawText());
            List<Card> deck = new List<Card>();
            string cardListRaw = deckInfo[3].CardList;
            string[] lines = cardListRaw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
            {
                int firstSpace = line.IndexOf(' ');
                int num = int.Parse(line.Substring(0, firstSpace));
                string name = line.Substring(firstSpace + 1);
                for (int i = 0; i < num; i++)
                {
                    var card = await _context.Cards
                        .FirstOrDefaultAsync(card => card.Name == name);
                    deck.Add(card);
                }
            }
            return deck;
        }


        [HttpGet("{DeckName}")]
        public async Task<ActionResult<CreateDeckDTO>> GetDeckByName(string deckName)
        {
            var deck = await _context.Decks
                .FirstOrDefaultAsync(deck => deck.DeckName == deckName);
            if (deck == null) return NotFound();

            return new CreateDeckDTO()
            {
                DeckName = deck.DeckName,
                Commander = deck.DeckCommander,
                Cards = deck.Cards,
            };
        }

        [HttpDelete("{DeckName}")]
        public async Task<ActionResult<Deck>> DeleteDeckByName(string deckName)
        {
            if(string.IsNullOrEmpty(deckName)) return BadRequest();
            Deck deckToRemove = await _context.Decks.FirstOrDefaultAsync(deck => deck.DeckName == deckName);

            if(deckToRemove == null) return NotFound();
            _context.Decks.Remove(deckToRemove);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{DeckName}")]
        public async Task<ActionResult<CreateDeckDTO>> RemakeDeck(string deckName, JsonElement json)
        {
            if (string.IsNullOrEmpty(deckName)) return BadRequest();
            if(json.ValueKind == JsonValueKind.Null) return BadRequest();

            var deckInfo = JsonSerializer.Deserialize<CreateDeckDTO[]>(json.GetRawText());
            var deck = await _context.Decks
                .FirstOrDefaultAsync(deck => deck.DeckName == deckName);
            if(deck == null)return NotFound();

            deck.DeckName = deckInfo[1].DeckName;
            deck.DeckCommander = deckInfo[2].Commander;
            deck.Cards = await ListOfCardsAsync(json);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
