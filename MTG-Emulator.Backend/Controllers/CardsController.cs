using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        [HttpGet("{CardName}")]
        public async Task<ActionResult<GetCardDTO>> GetCardNameBy(MTGContext context, string CardName)
        {
            var Card = await context.Cards
                .FirstOrDefaultAsync(card => card.Name == CardName);

            if (Card == null) return NotFound();

            return new GetCardDTO()
            {
                CardId = Card.CardId,
                Name = Card.Name,
                OracleText = Card.OracleText,
                ImageURI = Card.ImageURI
            };
        }

    }
}
