using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace UnitTests.Backend
{
    public class TestCardController
    {
        private CardsController uut;
        private MTGContext context;


        //Creates a test server
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            context = new MTGContext(options);
            uut = new CardsController(context);
        }

        //Tears down test database
        [TearDown]
        public void TearDown()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [Test]
        public async Task GetCardByName_ExistingCard_ReturnsCard()
        {
            var testCard = CreateTestCard();

            context.Cards.Add(testCard);
            await context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");

            Assert.That(result.Value, Is.Not.Null, "Expected a card, but got null");
            var resultCard = result.Value;
            Assert.That(resultCard.Name, Is.EqualTo("Test"));
            Assert.That(resultCard.OracleText, Is.EqualTo("Test text"));
            Assert.That(resultCard.ImageUri, Is.EqualTo("http://Test.com"));
        }

        [Test]
        public async Task GetCardByName_NonExistingCard_ReturnsNotFound()
        {
            var result = await uut.GetCardByName("NonExistentCard");

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetCardByName_EmptyName_ReturnsNotFound()
        {
            var resultEmpty = await uut.GetCardByName("");

            Assert.That(resultEmpty.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task GetCardByName_NullName_ReturnsNotFound()
        {
            var resultNull = await uut.GetCardByName(null);

            Assert.That(resultNull.Result, Is.TypeOf<BadRequestResult>());
        }


        public Card CreateTestCard()
        {
            var testCard = new Card
            {
                Name = "Test",
                OracleText = "Test text",
                ImageUri = "http://Test.com",
            };

            return testCard;
        }
    }
}
