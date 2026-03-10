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
            List<Card> deck = new List<Card>();

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

            return new CreateDeckDTO()
            {
                PlayerName = PlayerName,
                DeckName = DeckName,
                Commander = Commander,
                Cards = deck,
            };
        }
    }
}
