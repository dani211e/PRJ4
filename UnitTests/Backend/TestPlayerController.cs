using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


    }
}
