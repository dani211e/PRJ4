using Microsoft.AspNetCore.Mvc;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;

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

        private static CardDto unwrap(ActionResult<CardDto> result)
        {
            var ok = result.Result as OkObjectResult;
            return ok?.Value as CardDto
                   ?? throw new AssertionException("Expected OkObjectResult with CardDto");
        }

        [Test]
        public async Task GetCardByName_ExistingCard_ReturnsCard()
        {
            var testCard = createTestCard();
            Context.Cards.Add(testCard);
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");
            var dto    = unwrap(result);

            Assert.Multiple(() =>
            {
                Assert.That(dto,                     Is.Not.Null);
                Assert.That(dto.Name,       Is.EqualTo("Test"));
                Assert.That(dto.OracleText, Is.EqualTo("Test text"));
                Assert.That(dto.ImageUri,   Is.EqualTo("http://Test.com"));
            });
        }

        [Test]
        public async Task GetCardByName_ExistingCard_AltFaceIsNull()
        {
            Context.Cards.Add(createTestCard());
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");
            var dto    = unwrap(result);

            Assert.That(dto.AltFace, Is.Null, "Expected AltFace to be null when card has none");
        }

        [Test]
        public async Task GetCardByName_ExistingCard_RelatedCardsIsEmpty()
        {
            Context.Cards.Add(createTestCard());
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");
            var dto    = unwrap(result);

            Assert.That(dto.RelatedCards, Is.Empty,
                "Expected RelatedCards to be empty when card has none");
        }

        [Test]
        public async Task GetCardByName_CardWithAltFace_MapsAltFaceCorrectly()
        {
            var testCard = createTestCard(withAltFace: true);
            Context.Cards.Add(testCard);
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");
            var dto    = unwrap(result);

            Assert.That(dto.AltFace, Is.Not.Null, "Expected AltFace to be populated");

            Assert.Multiple(() =>
            {
                Assert.That(dto.AltFace.Name,       Is.EqualTo("Test Alt Face"));
                Assert.That(dto.AltFace.OracleText, Is.EqualTo("Alt face text"));
                Assert.That(dto.AltFace.ImageUri,   Is.EqualTo("http://TestAlt.com"));
            });
        }

        [Test]
        public async Task GetCardByName_CardWithRelatedCards_MapsRelatedCardsCorrectly()
        {
            var testCard = createTestCard(withRelatedCards: true);
            Context.Cards.Add(testCard);
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");
            var dto    = unwrap(result);

            Assert.That(dto.RelatedCards, Has.Count.EqualTo(1),
                "Expected one related card");

            var relatedCard = dto.RelatedCards[0];

            Assert.Multiple(() =>
            {
                Assert.That(relatedCard.Name,     Is.EqualTo("Related Card"));
                Assert.That(relatedCard.ImageUri, Is.EqualTo("http://Related.com"));
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
        public async Task GetCardByName_EmptyName_ReturnsBadRequest(string? cardName)
        {
            var result = await uut.GetCardByName(cardName!);

            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        private static Card createTestCard(bool withAltFace = false, bool withRelatedCards = false)
        {
            return new Card
            {
                Name       = "Test",
                OracleText = "Test text",
                ImageUri   = "http://Test.com",

                AltFace = withAltFace
                    ? new CardFace
                    {
                        Name       = "Test Alt Face",
                        OracleText = "Alt face text",
                        ImageUri   = "http://TestAlt.com",
                    }
                    : null,

                RelatedCards = withRelatedCards
                    ? new List<RelatedCard>
                    {
                        new RelatedCard
                        {
                            Name     = "Related Card",
                            ImageUri = "http://Related.com",
                        }
                    }
                    : new List<RelatedCard>()
            };
        }
    }
}