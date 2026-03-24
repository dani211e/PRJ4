using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace UnitTests.Backend
{
    public class TestTokenController
    {
        private MTGContext context;
        private TokenController uut;

        //Creates a test server
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase("TestDb")
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
            var testToken = this.testToken();
            context.RelatedCards.Add(testToken);
            await context.SaveChangesAsync();

            var result = await uut.GetTokenByName("Germ");

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var resultToken = okResult.Value as RelatedCardDto;
            Assert.That(resultToken.Name, Is.EqualTo("Germ"));
            Assert.That(resultToken.Uri, Is.EqualTo("http://Test.com"));
        }

        [Test]
        public async Task GetTokenByName_NonExistingToken_ReturnsNull()
        {
            var result = await uut.GetTokenByName("DoesNotExist");
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetTokenByName_NullToken_ReturnsToken()
        {
            var result = await uut.GetTokenByName(null);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetTokenByName_EmptyToken_ReturnsToken()
        {
            var result = await uut.GetTokenByName("");
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }


        private RelatedCard testToken()
        {
            var testCard = new Card
            {
                Name = "Test",
                OracleText = "Test text",
                ImageUri = "http://Test.com"
            };

            var testToken = new RelatedCard
            {
                Name = "Germ",
                URI = "http://Test.com",
                Card = testCard
            };
            return testToken;
        }
    }
}
