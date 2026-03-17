using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Backend
{
    public class TestTokenController
    {
        private TokenController uut;
        private MTGContext context;

        //Creates a test server
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            context = new MTGContext(options);
            uut = new TokenController(context);
        }

        //Tears down test database
        [TearDown]
        public void TearDown()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }


        [Test]
        public async Task GetTokenByName_ExistingToken_ReturnsToken()
        {
            var testCard = new Card
            {
                Name = "Test",
                OracleText = "Test text",
                ImageURI = "http://Test.com",
            };

            var testToken = new RelatedCard
            {
                Name = "Germ",
                URI = "http://Test.com",
                Card = testCard
            };

            context.RelatedCards.Add(testToken);
            await context.SaveChangesAsync();

            var  result = await uut.GetTokenByName("Germ");

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var resultToken = okResult.Value as RelatedCard;
            Assert.That(resultToken.Name, Is.EqualTo("Germ"));
        }

        [Test]
        public async Task GetTokenByName_NonExistingToken_ReturnsNull()
        {
            var response = await uut.GetTokenByName("DoesNotExist");
            Assert.That(response.Result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
