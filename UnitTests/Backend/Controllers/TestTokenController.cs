using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.RelatedCardsDTO;
using NUnit.Framework;

namespace UnitTests.Backend.Controllers
{
    public class TestTokenController : TestControllerBase
    {
        private TokenController uut;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            uut = new TokenController(Context);
        }

        [Test]
        public async Task GetTokenByName_ExistingToken_ReturnsToken()
        {
            var testToken = createTestToken();
            Context.RelatedCards.Add(testToken);
            await Context.SaveChangesAsync();

            var result = await uut.GetTokenByName("Germ");

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var resultToken = okResult.Value as RelatedCardDto;
            Assert.That(resultToken!.Name, Is.EqualTo("Germ"));
            Assert.That(resultToken.ImageUri, Is.EqualTo("http://Test.com"));
        }

        [TestCase("DoesNotExist")]
        [TestCase(null)]
        [TestCase("")]
        public async Task GetTokenByName_InvalidInput_ReturnNotFound(string? tokenName)
        {
            var result = await uut.GetTokenByName(tokenName!);
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
                ImageUri = "http://Test.com",
                Card = testCard,
            };
        }
    }
}
