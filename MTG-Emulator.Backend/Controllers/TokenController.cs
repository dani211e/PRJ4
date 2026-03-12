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
        private readonly MTGContext context;
        public TokenController(MTGContext context)
        {
            context = context;
        }

        [HttpGet("{tokenName}")]
        public async Task<ActionResult<RelatedCard>> GetTokenByName(string tokenName)
        {
            var token = await context.RelatedCards
                .FirstOrDefaultAsync(token => token.Name == tokenName);

            if (token == null) return NotFound();

            return Ok(token);
        }
    }
}
