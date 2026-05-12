using System.Collections.Generic;
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

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name,        Is.EqualTo("Test"));
                Assert.That(result.Value.OracleText,  Is.EqualTo("Test text"));
                Assert.That(result.Value.ImageUri,    Is.EqualTo("http://Test.com"));
            });
        }

        [Test]
        public async Task GetCardByName_ExistingCard_AltFaceIsNull()
        {
            Context.Cards.Add(createTestCard());
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");

            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.AltFace, Is.Null, "Expected AltFace to be null when card has none");
        }

        [Test]
        public async Task GetCardByName_ExistingCard_RelatedCardsIsEmpty()
        {
            Context.Cards.Add(createTestCard());
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");

            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.RelatedCards, Is.Empty, "Expected RelatedCards to be empty when card has none");
        }

        [Test]
        public async Task GetCardByName_CardWithAltFace_MapsAltFaceCorrectly()
        {
            var testCard = createTestCard(withAltFace: true);
            Context.Cards.Add(testCard);
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");

            Assert.That(result.Value,         Is.Not.Null);
            Assert.That(result.Value.AltFace, Is.Not.Null, "Expected AltFace to be populated");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.AltFace.Name,       Is.EqualTo("Test Alt Face"));
                Assert.That(result.Value.AltFace.OracleText, Is.EqualTo("Alt face text"));
                Assert.That(result.Value.AltFace.ImageUri,   Is.EqualTo("http://TestAlt.com"));
            });
        }

        [Test]
        public async Task GetCardByName_CardWithRelatedCards_MapsRelatedCardsCorrectly()
        {
            var testCard = createTestCard(withRelatedCards: true);
            Context.Cards.Add(testCard);
            await Context.SaveChangesAsync();

            var result = await uut.GetCardByName("Test");

            Assert.That(result.Value,              Is.Not.Null);
            Assert.That(result.Value.RelatedCards, Has.Count.EqualTo(1), "Expected one related card");

            var relatedCard = result.Value.RelatedCards[0];
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
            if (cardName != null)
            {
                var result = await uut.GetCardByName(cardName);

                Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
            }
        }
        

        private static Card createTestCard(bool withAltFace = false, bool withRelatedCards = false)
        {
            return new Card
            {
                Name         = "Test",
                OracleText   = "Test text",
                ImageUri     = "http://Test.com",
                AltFace      = withAltFace ? new CardFace
                {
                    Name       = "Test Alt Face",
                    OracleText = "Alt face text",
                    ImageUri   = "http://TestAlt.com",
                } : null,
                RelatedCards = withRelatedCards
                    ? new List<RelatedCard>
                    {
                        new RelatedCard
                        {
                            Name     = "Related Card",
                            ImageUri = "http://Related.com",
                        }
                    }
                    : new List<RelatedCard>(),
            };
        }
    }
}