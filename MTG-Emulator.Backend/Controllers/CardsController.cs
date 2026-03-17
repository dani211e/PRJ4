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
        private readonly MTGContext _context;
        public CardsController(MTGContext context)
        {
            _context = context;
        }

        [HttpGet("{CardName}")]
        public async Task<ActionResult<GetCardDTO>> GetCardNameBy(string cardName)
        {
            var card = await _context.Cards
                .FirstOrDefaultAsync(card => card.Name == cardName);

            if (card == null) return NotFound();

            return new GetCardDTO()
            {
                CardId = card.CardId,
                Name = card.Name,
                OracleText = card.OracleText,
                ImageURI = card.ImageURI
            };
        }

    }
}
