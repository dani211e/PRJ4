using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using NUnit.Framework;

namespace UnitTests.Backend.Controllers
{
    public class TestCardController : TestControllerBase
    {
        private CardsController uut;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            uut = new CardsController(Context);
        }

        [Test]
        public async Task GetCardByName_ExistingCard_ReturnsCard()
        {
            var testCard = createTestCard();

            Context.Cards.Add(testCard);
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");

            Assert.That(result.Value, Is.Not.Null, "Expected a card, but got null");
            var resultCard = result.Value;

            Assert.Multiple(() =>
            {
                Assert.That(resultCard.Name, Is.EqualTo("Test"));
                Assert.That(resultCard.OracleText, Is.EqualTo("Test text"));
                Assert.That(resultCard.ImageUri, Is.EqualTo("http://Test.com"));
            });
        }

        [Test]
        public async Task GetCardByName_NonExistingCard_ReturnsNotFound()
        {
            var result = await uut.GetCardByName("NonExistentCard");

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task GetCardByName_EmptyName_ReturnsBadrequest(string? cardName)
        {
            var resultEmpty = await uut.GetCardByName(cardName!);

            Assert.That(resultEmpty.Result, Is.TypeOf<BadRequestResult>());
        }

        private static Card createTestCard()
        {
            return new Card
            {
                Name = "Test",
                OracleText = "Test text",
                ImageUri = "http://Test.com",
            };
        }
    }
}
