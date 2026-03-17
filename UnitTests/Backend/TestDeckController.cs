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
                .UseInMemoryDatabase(databaseName: "TestDb")
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
        public async Task CreateDeck_ValidInput_ReturnsCreatedDec()
        {
            createPlayer("Test player");
            createCard("Test card");
            createCard("Test card2");

            var dto = new CreateDeckDto
            {
                PlayerName = "Test player",
                DeckName = "Test deck",
                Commander = "Test commander",
                CardList = "1 Test card\n2 Test card2\n"
            };

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        }

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
                Name = "name",
                OracleText = "Test text",
                ImageUri = "http://Test.com"
            };

            context.Cards.Add(card);
            context.SaveChanges();

            return card;
        }


        private Deck createDeck(string deckName = "Test deck")
        {
            var player = createPlayer();
            var card = createCard("Test card");

            var deck = new Deck
            {
                DeckName = deckName,
                DeckCommander = "Commander",
                Player = player,
                Cards = new List<Card> { card }
            };

            context.Decks.Add(deck);
            context.SaveChanges();

            return deck;
        }
    }
}
