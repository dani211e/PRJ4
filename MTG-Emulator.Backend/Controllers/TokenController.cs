using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        [HttpGet("{tokenName}")]
        public async Task<ActionResult<RelatedCard>> GetTokenByName(MTGContext context, string TokenName)
        {
            var Token = await context.RelatedCards
                .FirstOrDefaultAsync(token => token.Name == TokenName);

            if (Token == null) return NotFound();

            return Ok(Token);
        }
    }
}
