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
        [HttpPost]
        public async Task<ActionResult<CreateDeckDTO>> CreateDeck(MTGContext context, [FromBody] JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Null) return BadRequest();
            var DeckInfo = JsonSerializer.Deserialize<CreateDeckDTO[]>(json.GetRawText());

            string PlayerName = DeckInfo[0].PlayerName;
            string DeckName = DeckInfo[1].DeckName;
            string Commander =  DeckInfo[2].Commander;
            string CardListRaw = DeckInfo[3].CardList;

            return new CreateDeckDTO()
            {
                PlayerName = PlayerName,
                DeckName = DeckName,
                Commander = Commander,
                Cards = await ListOfCardsAsync(context, json),
            };
        }

        public async Task<List<Card>> ListOfCardsAsync(MTGContext context, JsonElement json)
        {
            var DeckInfo = JsonSerializer.Deserialize<CreateDeckDTO[]>(json.GetRawText());
            List<Card> deck = new List<Card>();
            string CardListRaw = DeckInfo[3].CardList;
            string[] lines = CardListRaw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
            {
                int firstSpace = line.IndexOf(' ');
                int num = int.Parse(line.Substring(0, firstSpace));
                string name = line.Substring(firstSpace + 1);
                for (int i = 0; i < num; i++)
                {
                    var Card = await context.Cards
                        .FirstOrDefaultAsync(card => card.Name == name);
                    deck.Add(Card);
                }
            }
            return deck;
        }


        [HttpGet("{DeckName}")]
        public async Task<ActionResult<CreateDeckDTO>> GetDeckNameBy(MTGContext context, string DeckName)
        {
            var Deck = await context.Decks
                .FirstOrDefaultAsync(deck => deck.DeckName == DeckName);
            if (Deck == null) return NotFound();

            return new CreateDeckDTO()
            {
                DeckName = Deck.DeckName,
                Commander = Deck.DeckCommander,
                Cards = Deck.Cards,
            };
        }

        [HttpDelete("{DeckName}")]
        public async Task<ActionResult<Deck>> GetDeckByName(MTGContext context, string DeckName)
        {
            if(string.IsNullOrEmpty(DeckName)) return BadRequest();
            Deck DeckToRemove = await context.Decks.FirstOrDefaultAsync(deck => deck.DeckName == DeckName);

            if(DeckToRemove == null) return NotFound();
            context.Decks.Remove(DeckToRemove);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{DeckName}")]
        public async Task<ActionResult<CreateDeckDTO>> RemakeDeck(MTGContext context, string DeckName, JsonElement json)
        {
            if (string.IsNullOrEmpty(DeckName)) return BadRequest();
            if(json.ValueKind == JsonValueKind.Null) return BadRequest();

            var DeckInfo = JsonSerializer.Deserialize<CreateDeckDTO[]>(json.GetRawText());
            var Deck = await context.Decks
                .FirstOrDefaultAsync(deck => deck.DeckName == DeckName);
            if(Deck == null)return NotFound();

            Deck.DeckName = DeckInfo[1].DeckName;
            Deck.DeckCommander = DeckInfo[2].Commander;
            Deck.Cards = await ListOfCardsAsync(context, json);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
