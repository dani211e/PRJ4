using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Unity.Db.DTO.RelatedCardsDTO;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PlayerOrAdmin")]
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
                ImageUri = token.ImageUri,
            });
        }
    }
}
