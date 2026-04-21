using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;
using NUnit.Framework;

namespace UnitTests.Backend
{
    public class TestPlayerController
    {
        private MTGContext context;
        private PlayerController uut;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            context = new MTGContext(options);
            uut = new PlayerController(context);
        }

        //Tears down test database
        [TearDown]
        public void TearDown()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [Test]
        public async Task CreateProfile_NewProfile_ReturnPlayer()
        {
            var result = await uut.CreateProfile("testPlayer", "testPassword");

            var createdResult = result.Result as CreatedAtActionResult;
            var player = createdResult!.Value as Player;

            Assert.That(player, Is.Not.Null);
            Assert.That(player.Username, Is.EqualTo("testPlayer"));
            Assert.That(player.Password, Is.EqualTo("testPassword"));
        }

        [Test]
        public async Task CreateProfile_ExistingProfile_ReturnPlayer()
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();
            var result = await uut.CreateProfile("testPlayer", "testPassword");

            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
        }

        [TestCase(null, "testPassword")]
        [TestCase("", "testPassword")]
        [TestCase("testPlayer", null)]
        [TestCase("testPlayer", "")]
        public async Task CreateProfile_InvalidInput_ReturnBadRequest(string? playerName, string? password)
        {
            var result = await uut.CreateProfile(playerName, password);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }


        [Test]
        public async Task GetProfile_ExistingProfile_ReturnPlayer()
        {
            var testPlayer = createTestPlayer();

            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.GetProfile("testPlayer");

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var resultPlayer = okResult.Value as PlayerDTO;
            Assert.That(resultPlayer!.Username, Is.EqualTo("testPlayer"));
        }

        [Test]
        public async Task GetProfile_NonExistingProfile_ReturnNull()
        {
            var result = await uut.GetProfile("DoesNotExist");
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task GetProfile_InvalidInput_ReturnBadRequest(string? playerName)
        {
            var result = await uut.GetProfile(playerName);
            Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task DeleteProfile_ExistingProfile_ReturnPlayer()
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.DeleteProfile("testPlayer");
            await context.SaveChangesAsync();

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteProfile_NonExistingProfile_ReturnNull()
        {
            var result = await uut.DeleteProfile("DoesNotExist");
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task DeleteProfile_InvalidInput_ReturnBadRequest(string? playerName)
        {
            var result = await uut.DeleteProfile(playerName);
            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [TestCase(1)]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task UpdateProfile_ExistingProfileInt_ReturnPlayer(int input)
        {
            var endGameResult = (GameResults)input;
            await verifyStatsUpdated(endGameResult);
        }

        [TestCase("Win")]
        [TestCase("Draw")]
        [TestCase("Loss")]
        public async Task UpdateProfile_ExistingProfilestring_ReturnPlayer(string input)
        {
            Enum.TryParse(input, out GameResults endGameResult);
            await verifyStatsUpdated(endGameResult);
        }

        [TestCase(GameResults.Win)]
        [TestCase(GameResults.Draw)]
        [TestCase(GameResults.Loss)]
        public async Task UpdateProfile_NonExistingProfile_ReturnNull(GameResults endGameResult)
        {
            var result = await uut.UpdatePlayerStats("DoesNotExist", endGameResult);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateProfile_NullProfile_ReturnNull()
        {
            var result = await uut.UpdatePlayerStats(null!, GameResults.Win);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateProfile_EmptyProfile_ReturnNull()
        {
            var result = await uut.UpdatePlayerStats("", GameResults.Win);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase(-3)]
        [TestCase(-2)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task UpdateProfile_invalidGameResult_ReturnBadRequest(int input)
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var endGameResult = (GameResults)input;
            var result = await uut.UpdatePlayerStats(testPlayer.Username, endGameResult);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_NewPassword_ReturnPlayer()
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, "testPassword");
            await context.SaveChangesAsync();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_NullPassword_ReturnNull()
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, null!);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_EmptyPassword_ReturnNull()
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, "");
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_WhiteSpacePassword_ReturnBadRequest()
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, " ");
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        private async Task verifyStatsUpdated(GameResults endGameResult)
        {
            var testPlayer = createTestPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.UpdatePlayerStats(testPlayer.Username, endGameResult);
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        private static Player createTestPlayer()
        {
            return new Player
            {
                Username = "testPlayer",
                Password = "testPassword",
                GamesWon = 5,
                GamesLost = 5,
                GamesDrawn = 5,
            };
        }
    }
}
