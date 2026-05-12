using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;
using NUnit.Framework;

namespace UnitTests.Backend.Controllers
{
    public class TestPlayerController : TestControllerBase
    {
        private PlayerController uut;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            uut = new PlayerController(Context);
            setControllerUser(uut, "test-api-user-id");
        }

        // GetProfile

        [Test]
        public async Task GetProfile_ExistingPlayer_ReturnsCorrectProfile()
        {
            await insertPlayerAsync();

            var result = await uut.GetProfile("Test player");
            var ok  = result.Result as OkObjectResult;
            var dto = ok?.Value as PlayerDto;

            Assert.Multiple(() =>
            {
                Assert.That(dto,             Is.Not.Null);
                Assert.That(dto!.Username,   Is.EqualTo("Test player"));
                Assert.That(dto.GamesWon,    Is.EqualTo(0));
                Assert.That(dto.GamesLost,   Is.EqualTo(0));
                Assert.That(dto.GamesDrawed, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task GetProfile_PlayerDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.GetProfile("NonExistingPlayer");

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        // DeleteProfile

        [Test]
        public async Task DeleteProfile_ExistingPlayer_DeletesPlayer()
        {
            await insertPlayerAsync();

            var result = await uut.DeleteProfile("Test player");
            Assert.That(result, Is.TypeOf<NoContentResult>());

            var deleted = await Context.Players.FirstOrDefaultAsync(p => p.Username == "Test player");
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task DeleteProfile_PlayerDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.DeleteProfile("NonExistingPlayer");

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteProfile_CallerIsNotOwner_ReturnsForbid()
        {
            await insertPlayerAsync(apiUserId: "other-user-id");

            var result = await uut.DeleteProfile("Test player");

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        // UpdatePlayerStats

        [Test]
        public async Task UpdatePlayerStats_Win_IncrementsGamesWon()
        {
            await insertPlayerAsync();

            var result = await uut.UpdatePlayerStats("Test player", GameResults.Win);
            Assert.That(result.Result, Is.TypeOf<NoContentResult>());

            var player = await Context.Players.FirstAsync(p => p.Username == "Test player");
            Assert.Multiple(() =>
            {
                Assert.That(player.GamesWon,   Is.EqualTo(1));
                Assert.That(player.GamesLost,  Is.EqualTo(0));
                Assert.That(player.GamesDrawn, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task UpdatePlayerStats_Loss_IncrementsGamesLost()
        {
            await insertPlayerAsync();

            var result = await uut.UpdatePlayerStats("Test player", GameResults.Loss);
            Assert.That(result.Result, Is.TypeOf<NoContentResult>());

            var player = await Context.Players.FirstAsync(p => p.Username == "Test player");
            Assert.Multiple(() =>
            {
                Assert.That(player.GamesWon,   Is.EqualTo(0));
                Assert.That(player.GamesLost,  Is.EqualTo(1));
                Assert.That(player.GamesDrawn, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task UpdatePlayerStats_Draw_IncrementsGamesDrawn()
        {
            await insertPlayerAsync();

            var result = await uut.UpdatePlayerStats("Test player", GameResults.Draw);
            Assert.That(result.Result, Is.TypeOf<NoContentResult>());

            var player = await Context.Players.FirstAsync(p => p.Username == "Test player");
            Assert.Multiple(() =>
            {
                Assert.That(player.GamesWon,   Is.EqualTo(0));
                Assert.That(player.GamesLost,  Is.EqualTo(0));
                Assert.That(player.GamesDrawn, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task UpdatePlayerStats_PlayerDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.UpdatePlayerStats("NonExistingPlayer", GameResults.Win);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdatePlayerStats_CallerIsNotOwner_ReturnsForbid()
        {
            await insertPlayerAsync(apiUserId: "other-user-id");

            var result = await uut.UpdatePlayerStats("Test player", GameResults.Win);

            Assert.That(result.Result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task UpdatePlayerStats_MultipleResults_AccumulatesCorrectly()
        {
            await insertPlayerAsync();

            await uut.UpdatePlayerStats("Test player", GameResults.Win);
            await uut.UpdatePlayerStats("Test player", GameResults.Win);
            await uut.UpdatePlayerStats("Test player", GameResults.Loss);
            await uut.UpdatePlayerStats("Test player", GameResults.Draw);

            var player = await Context.Players.FirstAsync(p => p.Username == "Test player");
            Assert.Multiple(() =>
            {
                Assert.That(player.GamesWon,   Is.EqualTo(2));
                Assert.That(player.GamesLost,  Is.EqualTo(1));
                Assert.That(player.GamesDrawn, Is.EqualTo(1));
            });
        }
        
        [Test]
        public async Task UpdatePlayerStats_InvalidGameResult_ReturnsBadRequest()
        {
            await insertPlayerAsync();

            var result = await uut.UpdatePlayerStats("Test player", (GameResults)999);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        // Helpers

        private static void setControllerUser(ControllerBase controller, string apiUserId, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiUserId),
            };

            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var identity  = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private async Task<Player> insertPlayerAsync(
            string username  = "Test player",
            string apiUserId = "test-api-user-id")
        {
            var player = new Player
            {
                Username  = username,
                ApiUserId = apiUserId,
                GamesWon  = 0,
                GamesLost = 0,
                GamesDrawn = 0,
            };
            Context.Players.Add(player);
            await Context.SaveChangesAsync();
            return player;
        }
    }
}