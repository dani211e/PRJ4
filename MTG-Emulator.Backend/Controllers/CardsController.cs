using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Shared.Db.DTO.CardDTO;
using MTG_Emulator.Shared.Db.DTO.CardFaceDTO;
using MTG_Emulator.Shared.Db.DTO.RelatedCardDTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PlayerOrAdmin")]
    public class CardsController : MtgController
    {
        private readonly MTGContext context;

        public CardsController(MTGContext context)
        {
            this.context = context;
        }

        [HttpGet("{cardName}")]
        public async Task<ActionResult<CardDto>> GetCardByName(string cardName)
        {
            if (string.IsNullOrEmpty(cardName))
                return BadRequest();

            var card = await context.Cards
                .Include(c => c.AltFace)
                .Include(c => c.RelatedCards)
                .FirstOrDefaultAsync(c => c.Name == cardName);

            if (card == null)
                return NotFound();

            return Ok(ToCardDto(card));
        }
    }
}
