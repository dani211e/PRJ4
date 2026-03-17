using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly MTGContext context;

        public CardsController(MTGContext context)
        {
            this.context = context;
        }

        [HttpGet("{CardName}")]
        public async Task<ActionResult<CardDto>> GetCardByName(string cardName)
        {
            if (string.IsNullOrEmpty(cardName)) return BadRequest();

            var card = await context.Cards
                .FirstOrDefaultAsync(card => card.Name == cardName);

            if (card == null) return NotFound();

            return new CardDto
            {
                CardId = card.CardId,
                Name = card.Name,
                OracleText = card.OracleText,
                ImageUri = card.ImageURI
            };
        }
    }
}
