using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.DTO.RelatedCardsDTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly MTGContext context;

        public TokenController(MTGContext context)
        {
            this.context = context;
        }

        [HttpGet("{tokenName}")]
        public async Task<ActionResult<RelatedCardDto>> GetTokenByName(string tokenName)
        {
            var token = await context.RelatedCards
                .FirstOrDefaultAsync(t => t.Name == tokenName);

            if (token == null)
                return NotFound();

            return Ok(new RelatedCardDto
            {
                RelatedCardId = token.RelatedCardId,
                Name = token.Name,
                Uri = token.URI,
            });
        }
    }
}
