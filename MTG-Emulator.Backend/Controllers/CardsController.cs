using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.CardFace;
using MTG_Emulator.Unity.Db.DTO.RelatedCardsDTO;

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
            if (string.IsNullOrEmpty(cardName))
                return BadRequest();

            var card = await context.Cards
                .Include(c => c.AltFace)
                .Include(c => c.RelatedCards)
                .FirstOrDefaultAsync(c => c.Name == cardName);

            if (card == null)
                return NotFound();

            return new CardDto
            {
                CardId = card.CardId,
                ScryfallId = card.ScryfallId,
                Name = card.Name,
                OracleText = card.OracleText,
                ImageUri = card.ImageUri,
                AltFace = card.AltFace == null ? null : new CardFaceDto
                {
                    Name = card.AltFace.Name,
                    OracleText = card.AltFace.OracleText,
                    ImageUri = card.AltFace.ImageUri,
                },
                RelatedCards = card.RelatedCards.Select(rc => new RelatedCardDto
                {
                    RelatedCardId = rc.RelatedCardId,
                    Name = rc.Name,
                    ImageUri = rc.ImageUri,
                }).ToList()
            };
        }
    }
}
