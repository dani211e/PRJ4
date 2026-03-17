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
        List<RelatedCard> _relatedCards = new List<RelatedCard>();
        private readonly MTGContext _context;
        public TokenController(MTGContext context)
        {
            _context = context;
        }

        public TokenController(List<RelatedCard> relatedCards)
        {
            _relatedCards = relatedCards;
        }

        [HttpGet("{tokenName}")]
        public async Task<ActionResult<RelatedCard>> GetTokenByName(string tokenName)
        {
            var token = await _context.RelatedCards
                .FirstOrDefaultAsync(token => token.Name == tokenName);

            if (token == null) return NotFound();

            return Ok(token);
        }


    }
}
