using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;

namespace UnitTests.Backend.Controllers
{
    public abstract class TestControllerBase
    {
        protected MTGContext Context = null!;

        [SetUp]
        public virtual void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new MTGContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }

        protected static void SetControllerUser(ControllerBase controller, string apiUserId, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiUserId),
            };

            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };
        }

        protected async Task<Player> InsertPlayerAsync(
            string username  = "Test player",
            string apiUserId = "test-api-user-id")
        {
            var player = new Player
            {
                Username   = username,
                ApiUserId  = apiUserId,
                GamesWon   = 0,
                GamesLost  = 0,
                GamesDrawn = 0,
            };
            Context.Players.Add(player);
            await Context.SaveChangesAsync();
            return player;
        }

        protected async Task<Card> InsertCardAsync(string name)
        {
            var card = new Card
            {
                Name         = name,
                OracleText   = "Test text",
                ImageUri     = "http://Test.com",
                RelatedCards = new List<RelatedCard>(),
            };
            Context.Cards.Add(card);
            await Context.SaveChangesAsync();
            return card;
        }
    }
}