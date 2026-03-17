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
        private readonly MTGContext _context;

        public TokenController(MTGContext context)
        {
            _context = context;
        }

        [HttpGet("{tokenName}")]
        public async Task<ActionResult<RelatedCardDto>> GetTokenByName(string tokenName)
        {
            var token = await _context.RelatedCards
                .FirstOrDefaultAsync(t => t.Name == tokenName);

            if (token == null) return NotFound();

            return Ok(new RelatedCardDto
            {
                RelatedCardId = token.RelatedCardId,
                Name = token.Name,
                Uri = token.URI
            });
        }
    }
}
