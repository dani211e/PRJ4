using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB.DTO;

namespace UnitTests.Backend
{
    public class TestPlayerController
    {
        private PlayerController uut;
        private MTGContext context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
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

            var player  = createdResult.Value as Player;

            Assert.That(player, Is.Not.Null);
            Assert.That(player.Username, Is.EqualTo("testPlayer"));
            Assert.That(player.Password, Is.EqualTo("testPassword"));
        }

        [Test]
        public async Task CreateProfile_ExistingProfile_ReturnPlayer()
        {
            var testPlayer = this.testPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();
            var result = await uut.CreateProfile("testPlayer", "testPassword");

            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
        }

        [Test]
        public async Task CreateProfile_NullUserName_ReturnNotFound()
        {
            var result = await uut.CreateProfile(null, "testPassword");
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateProfile_EmptyUserName_ReturnNotFound()
        {
            var result = await uut.CreateProfile("", "testPassword");
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateProfile_NullPassword_ReturnNotFound()
        {
            var result = await uut.CreateProfile("testPlayer", null);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateProfile_EmptyPassword_ReturnNotFound()
        {
            var result = await uut.CreateProfile("testPlayer", "");
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetProfile_ExistingProfile_ReturnPlayer()
        {
            var testPlayer = this.testPlayer();

            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.GetProfile("testPlayer");

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var resultPlayer = okResult.Value as PlayerDto;
            Assert.That(resultPlayer.Username, Is.EqualTo("testPlayer"));
        }

        [Test]
        public async Task GetProfile_NonExistingProfile_ReturnNull()
        {
            var result = await uut.GetProfile("DoesNotExist");
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetProfile_NullUsername_ReturnNotFound()
        {
            var resultNull = await uut.GetProfile(null);

            Assert.That(resultNull.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task GetProfile_EmptyUsername_ReturnNotFound()
        {
            var resultEmpty = await uut.GetProfile("");

            Assert.That(resultEmpty.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task DeleteProfile_ExistingProfile_ReturnPlayer()
        {
            var testPlayer = this.testPlayer();

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

        [Test]
        public async Task DeleteProfile_EmptyUsername_ReturnNotFound()
        {
            var result = await uut.DeleteProfile("");
            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task DeleteProfile_NullUsername_ReturnNotFound()
        {
            var result = await uut.DeleteProfile(null);
            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [TestCase(1)]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task UpdateProfile_ExistingProfileInt_ReturnPlayer(int input)
        {
            var endGameResult = (PlayerController.GameResults)input;
            await AssertUpdateStats(endGameResult);
        }

        [TestCase("Win")]
        [TestCase("Draw")]
        [TestCase("Loss")]
        public async Task UpdateProfile_ExistingProfilestring_ReturnPlayer(string input)
        {
            Enum.TryParse(input, out PlayerController.GameResults endGameResult);
            await AssertUpdateStats(endGameResult);
        }

        [TestCase(PlayerController.GameResults.Win)]
        [TestCase(PlayerController.GameResults.Draw)]
        [TestCase(PlayerController.GameResults.Loss)]
        public async Task UpdateProfile_NonExistingProfile_ReturnNull(PlayerController.GameResults endGameResult)
        {
            var result = await uut.UpdatePlayerStats("DoesNotExist", endGameResult);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateProfile_NullProfile_ReturnNull()
        {
            var result = await uut.UpdatePlayerStats(null, PlayerController.GameResults.Win);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateProfile_EmptyProfile_ReturnNull()
        {
            var result = await uut.UpdatePlayerStats("", PlayerController.GameResults.Win);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase(-3)]
        [TestCase(-2)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task UpdateProfile_invalidGameResult_ReturnBadRequest(int input)
        {
            var testPlayer = this.testPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var endGameResult = (PlayerController.GameResults)input;
            var result = await uut.UpdatePlayerStats(testPlayer.Username, endGameResult);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_NewPassword_ReturnPlayer()
        {
            var testPlayer = this.testPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, "testPassword");
            await context.SaveChangesAsync();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_NullPassword_ReturnNull()
        {
            var testPlayer = this.testPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, null);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPlayerPassword_EmptyPassword_ReturnNull()
        {
            var testPlayer = this.testPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.ResetPlayerPassword(testPlayer.Username, "");
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        private async Task AssertUpdateStats(PlayerController.GameResults endGameResult)
        {
            var testPlayer = this.testPlayer();
            context.Players.Add(testPlayer);
            await context.SaveChangesAsync();

            var result = await uut.UpdatePlayerStats(testPlayer.Username,endGameResult);
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        public Player testPlayer()
        {
            var testPlayer = new Player
            {
                Username = "testPlayer",
                Password = "testPassword",
                GamesWon = 5,
                GamesLost = 5,
                GamesDrawed = 5
            };
            return testPlayer;
        }
    }
}
