using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;

namespace UnitTests.Backend
{
    public class TestDeckController
    {
        private DeckController uut;
        private MTGContext context;

        public Deck CreateTestDeck()
        {

            var testDeck = new Deck
            {
                DeckId = 1,
                DeckName =  "Test Deck",
                DeckCommander =  "Test Deck Commander",
                Cards = new List<Card>
                {
                    new Card()
                    {
                        CardId = 1,
                        Name = "Test Card 1",
                        OracleText = "Test1 text",
                        ImageUri = "http://Test1.com"
                    },
                    new Card()
                    {
                        CardId = 2,
                        Name = "Test Card 2",
                        OracleText = "Test2 text",
                        ImageUri = "http://Test2.com"
                    },
                    new Card()
                    {
                        CardId = 3,
                        Name = "Test Card 3",
                        OracleText = "Test3 text",
                        ImageUri = "http://Test3.com"
                    }
                },
                Player = new Player
                {
                    PlayerId = 1,
                    GamesWon = 0,
                    GamesDrawed = 0,
                    GamesLost = 0,
                    Username = "Test Username",
                    Password = "Test Password"
                }
            };

            return testDeck;
        }

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
        public void CreateDeck_NewDeck_ReturnsNewDeck()
        {

            Assert.Pass();
        }
    }
}
