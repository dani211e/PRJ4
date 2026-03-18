using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.DTO;
using MTG_Emulator.Backend.DB.Models;

namespace UnitTests.Backend
{
    public class TestDeckController
    {
        private DeckController uut;
        private MTGContext context;

        //Creates a test server
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            context = new MTGContext(options);
            uut = new DeckController(context);
        }

        //Tears down test database
        [TearDown]
        public void TearDown()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [Test]
        public async Task CreateDeck_ValidInput_CreatesDeckWithCorrectCards()
        {
            createPlayer();
            createCard("Test card");
            createCard("Test card2");

            var dto = createDeckDto();

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.Multiple(() =>
            {
                Assert.That(deck.DeckName, Is.EqualTo("Test deck"));
                Assert.That(deck.DeckCommander, Is.EqualTo("Test commander"));

                Assert.That(deck.Cards, Has.Count.EqualTo(3));
                Assert.That(deck.Cards.Count(c => c.Name == "Test card"), Is.EqualTo(1));
                Assert.That(deck.Cards.Count(c => c.Name == "Test card2"), Is.EqualTo(2));
            });
        }

        [Test]
        public async Task CreateDeck_PlayerDoesNotExist_ReturnsBadRequest()
        {
            createCard("Test card");
            var dto = createDeckDto();

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_CardDoesNotExist_ReturnsBadRequest()
        {
            createPlayer();
            var dto = createDeckDto();

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [TestCase("1 Test card\n", 1)]
        [TestCase("2 Test card\n", 2)]
        [TestCase("3 Test card\n", 3)]
        public async Task CreateDeck_ParsesCardQuantitiesCorrectly(string cardList, int expectedCount)
        {
            createPlayer();
            createCard("Test card");

            var dto = createDeckDto(cardList: cardList);

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.That(deck.Cards.Count, Is.EqualTo(expectedCount));
        }

        [TestCase((string)null, "Test player", "1 Test card\n")]
        [TestCase("", "Test player", "1 Test card\n")]
        [TestCase("Test deck", (string)null, "1 Test card\n")]
        [TestCase("Test deck", "", "1 Test card\n")]
        [TestCase("Test deck", "Test player", (string)null)]
        [TestCase("Test deck", "Test player", "")]
        public async Task CreateDeck_InvalidInput_ReturnsBadRequest(
            string? deckName,
            string? playerName,
            string? cardList)
        {
            var dto = new CreateDeckDto
            {
                DeckName = deckName,
                PlayerName = playerName,
                CardList = cardList
            };

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_InvalidCardLineFormat_ReturnsBadRequest()
        {
            createPlayer();
            var dto = createDeckDto(cardList: "InvalidLineWithoutSpace");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_InvalidQuantity_ReturnsBadRequest()
        {
            createPlayer();
            var dto = createDeckDto(cardList: "X Test card");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_MultipleInvalidCards_ReturnsAllInvalidNames()
        {
            createPlayer();
            var dto = createDeckDto(cardList: "1 Test card\n2 Test card2\n");

            var result = await uut.CreateDeck(dto);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);

            var value = badRequest.Value as InvalidCardsResponse;
            Assert.That(value, Is.Not.Null);
            Assert.That(value.InvalidCards, Does.Contain("Test card"));
            Assert.That(value.InvalidCards, Does.Contain("Test card2"));
        }

        [Test]
        public async Task CreateDeck_DuplicateCardLines_AreSummedCorrectly()
        {
            createPlayer();
            createCard("Test card");

            var dto = createDeckDto(cardList: "1 Test card\n2 Test card\n");

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.That(deck.Cards.Count(c => c.Name == "Test card"), Is.EqualTo(3));
        }

        [Test]
        public async Task CreateDeck_HandlesEmptyLinesInCardList()
        {
            createPlayer();
            createCard("Test card");

            var dto = createDeckDto(cardList: "\n1 Test card\n\n");

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.That(deck.Cards.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateDeck_CaseMismatch_ReturnsBadRequest()
        {
            createPlayer();
            createCard("Test card");

            var dto = createDeckDto(cardList: "1 test card");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_PersistsDeckInDatabase()
        {
            createPlayer();
            createCard("Test card");

            var dto = createDeckDto(cardList: "1 Test card\n");

            await uut.CreateDeck(dto);

            var deck = context.Decks.Include(d => d.Cards).FirstOrDefault();

            Assert.That(deck, Is.Not.Null);
            Assert.That(deck.Cards.Count, Is.EqualTo(1));
        }


        // Helper functions

        private Player createPlayer(string username = "Test player")
        {
            var player = new Player
            {
                Username = username,
                GamesWon = 0,
                GamesLost = 0,
                GamesDrawed = 0,
                Password = "Test"
            };

            context.Players.Add(player);
            context.SaveChanges();
            return player;
        }

        private Card createCard(string name)
        {
            var card = new Card
            {
                Name = name,
                OracleText = "Test text",
                ImageUri = "http://Test.com"
            };

            context.Cards.Add(card);
            context.SaveChanges();
            return card;
        }

        private CreateDeckDto createDeckDto(
            string playerName = "Test player",
            string deckName = "Test deck",
            string commander = "Test commander",
            string cardList = "1 Test card\n2 Test card2\n")
        {
            return new CreateDeckDto
            {
                PlayerName = playerName,
                DeckName = deckName,
                Commander = commander,
                CardList = cardList
            };
        }

        private DeckDto extractCreatedDto(ActionResult<DeckDto> result)
        {
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created?.Value, Is.TypeOf<DeckDto>());

            return created.Value as DeckDto;
        }
    }
}
