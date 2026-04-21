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
    public class TestTokenController
    {
        private MTGContext context;
        private TokenController uut;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            context = new MTGContext(options);
            uut = new TokenController(context);
        }

        [TearDown]
        public void TearDown()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }


        [Test]
        public async Task GetTokenByName_ExistingToken_ReturnsToken()
        {
            var testToken = createTestToken();
            context.RelatedCards.Add(testToken);
            await context.SaveChangesAsync();

            var result = await uut.GetTokenByName("Germ");

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var resultToken = okResult.Value as RelatedCardDTO;
            Assert.That(resultToken!.Name, Is.EqualTo("Germ"));
            Assert.That(resultToken.Uri, Is.EqualTo("http://Test.com"));
        }

        [TestCase("DoesNotExist")]
        [TestCase(null)]
        [TestCase("")]
        public async Task GetTokenByName_InvalidInput_ReturnNotFound(string? tokenName)
        {
            var result = await uut.GetTokenByName(tokenName);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        private static RelatedCard createTestToken()
        {
            var testCard = new Card
            {
                Name = "Test",
                OracleText = "Test text",
                ImageUri = "http://Test.com",
            };

            return new RelatedCard
            {
                Name = "Germ",
                URI = "http://Test.com",
                Card = testCard,
            };
        }
    }
}
