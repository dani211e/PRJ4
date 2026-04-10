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
        private MTGContext context;
        private DeckController uut;

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
            createPlayerAsync();
            createCardAsync("Test card");
            createCardAsync("Test card2");

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
            createCardAsync("Test card");
            var dto = createDeckDto();

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_CardDoesNotExist_ReturnsBadRequest()
        {
            createPlayerAsync();
            var dto = createDeckDto();

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [TestCase("1 Test card\n", 1)]
        [TestCase("2 Test card\n", 2)]
        [TestCase("3 Test card\n", 3)]
        public async Task CreateDeck_ParsesCardQuantitiesCorrectly(string cardList, int expectedCount)
        {
            createPlayerAsync();
            createCardAsync("Test card");

            var dto = createDeckDto(cardList: cardList);

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.That(deck.Cards.Count, Is.EqualTo(expectedCount));
        }

        [TestCase(null, "Test player", "1 Test card\n")]
        [TestCase("", "Test player", "1 Test card\n")]
        [TestCase("Test deck", null, "1 Test card\n")]
        [TestCase("Test deck", "", "1 Test card\n")]
        [TestCase("Test deck", "Test player", null)]
        [TestCase("Test deck", "Test player", "")]
        public async Task CreateDeck_InvalidInput_ReturnsBadRequest(
            string? deckName,
            string? playerName,
            string? cardList)
        {
            var dto = new CreateDeckDTO
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
            createPlayerAsync();
            var dto = createDeckDto(cardList: "InvalidLineWithoutSpace");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_InvalidQuantity_ReturnsBadRequest()
        {
            createPlayerAsync();
            var dto = createDeckDto(cardList: "X Test card");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_MultipleInvalidCards_ReturnsAllInvalidNames()
        {
            createPlayerAsync();
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
        public async Task CreateDeck_AddsInvalidCardNames_WhenSomeCardsDoNotExist()
        {
            await createPlayerAsync();
            await createCardAsync("Valid Card");

            var dto = createDeckDto(cardList: "1 Valid Card\n2 Missing Card\n");

            var result = await uut.CreateDeck(dto);
            var badRequest = result.Result as BadRequestObjectResult;

            Assert.That(badRequest, Is.Not.Null);
            var value = badRequest.Value as InvalidCardsResponse;
            Assert.That(value, Is.Not.Null);
            Assert.That(value.InvalidCards, Does.Contain("Missing Card"));
        }

        [Test]
        public async Task CreateDeck_DuplicateCardLines_AreSummedCorrectly()
        {
            createPlayerAsync();
            createCardAsync("Test card");

            var dto = createDeckDto(cardList: "1 Test card\n2 Test card\n");

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.That(deck.Cards.Count(c => c.Name == "Test card"), Is.EqualTo(3));
        }

        [Test]
        public async Task CreateDeck_HandlesEmptyLinesInCardList()
        {
            createPlayerAsync();
            createCardAsync("Test card");

            var dto = createDeckDto(cardList: "\n1 Test card\n\n");

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.That(deck.Cards.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateDeck_CaseMismatch_ReturnsBadRequest()
        {
            createPlayerAsync();
            createCardAsync("Test card");

            var dto = createDeckDto(cardList: "1 test card");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_PersistsDeckInDatabase()
        {
            createPlayerAsync();
            createCardAsync("Test card");

            var dto = createDeckDto(cardList: "1 Test card\n");

            await uut.CreateDeck(dto);

            var deck = context.Decks.Include(d => d.Cards).FirstOrDefault();

            Assert.That(deck, Is.Not.Null);
            Assert.That(deck.Cards.Count, Is.EqualTo(1));
        }


        // Test GetByName

        [Test]
        public async Task GetDeckByName_ExistingDeck_ReturnsCorrectDeck()
        {
            var player = await createPlayerAsync();
            var card = await createCardAsync("Test card");

            var deck = new Deck
            {
                DeckName = "Test deck",
                DeckCommander = "Test commander",
                Player = player,
                Cards = new List<Card> { card }
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var result = await uut.GetDeckByName("Test deck");

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = result.Result as OkObjectResult;
            var deckDto = ok?.Value as DeckDTO;

            Assert.Multiple(() =>
            {
                Assert.That(deckDto, Is.Not.Null);
                Assert.That(deckDto.DeckName, Is.EqualTo("Test deck"));
                Assert.That(deckDto.DeckCommander, Is.EqualTo("Test commander"));
                Assert.That(deckDto.Cards.Count, Is.EqualTo(1));
                Assert.That(deckDto.Cards[0].Name, Is.EqualTo("Test card"));
            });
        }

        [Test]
        public async Task GetDeckByName_DeckDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.GetDeckByName("NonExistingDeck");
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase("")]
        [TestCase("Test deck")]
        public async Task GetDeckByName_InvalidDeckName_ReturnsNotFound(string deckName)
        {
            var result = await uut.GetDeckByName(deckName);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetDeckByName_DeckWithMultipleCards_ReturnsAllCards()
        {
            var player = await createPlayerAsync();
            var card1 = await createCardAsync("Test Card1");
            var card2 = await createCardAsync("Test Card2");

            var deck = new Deck
            {
                DeckName = "MultiCardDeck",
                DeckCommander = "Test Commander",
                Player = player,
                Cards = new List<Card> { card1, card1, card2 }
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var result = await uut.GetDeckByName("MultiCardDeck");
            var deckDto = (result.Result as OkObjectResult)?.Value as DeckDTO;

            Assert.Multiple(() =>
            {
                Assert.That(deckDto, Is.Not.Null);
                Assert.That(deckDto.DeckName, Is.EqualTo("MultiCardDeck"));
                Assert.That(deckDto.DeckCommander, Is.EqualTo("Test Commander"));
                Assert.That(deckDto.Cards.Count, Is.EqualTo(3));
                Assert.That(deckDto.Cards.Count(c => c.Name == "Test Card1"), Is.EqualTo(2));
                Assert.That(deckDto.Cards.Count(c => c.Name == "Test Card2"), Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetDeckByName_CaseSensitiveDeckName_ReturnsNotFound()
        {
            var player = await createPlayerAsync();
            var deck = new Deck
            {
                DeckName = "ExactCaseDeck",
                DeckCommander = "Test Commander",
                Player = player
            };
            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var result = await uut.GetDeckByName("exactcasedeck");
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }


        // Test DeleteDeckByName

        [Test]
        public async Task DeleteDeckByName_ExistingDeck_DeletesDeck()
        {
            var player = await createPlayerAsync();
            var deck = new Deck
            {
                DeckName = "DeckToDelete",
                DeckCommander = "Test Commander",
                Player = player
            };

            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var result = await uut.DeleteDeckByName("DeckToDelete");
            Assert.That(result, Is.TypeOf<NoContentResult>());

            var deleted = await context.Decks.FirstOrDefaultAsync(d => d.DeckName == "DeckToDelete");
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task DeleteDeckByName_DeckDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.DeleteDeckByName("NonExistingDeck");
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [TestCase("")]
        [TestCase(" ")]
        public async Task DeleteDeckByName_InvalidDeckName_ReturnsNotFound(string deckName)
        {
            var result = await uut.DeleteDeckByName(deckName ?? string.Empty);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        private UpdateDeckDTO UpdateDeckDto(
            string deckName = "Test deck",
            string commander = "Test commander",
            string cardList = "1 Test card\n2 Test card2\n")
        {
            return new UpdateDeckDTO
            {
                DeckName = deckName,
                Commander = commander,
                CardList = cardList
            };
        }

        // Test Update deck
        [Test]
        public async Task UpdateDeck_ExistingDeck_UpdatesDeckAndCards()
        {
            var player = await createPlayerAsync();
            var card1 = await createCardAsync("Test Card1");
            var card2 = await createCardAsync("Test Card2");

            var deck = new Deck
            {
                DeckName = "DeckToUpdate",
                DeckCommander = "OldCommander",
                Player = player,
                Cards = new List<Card> { card1 }
            };
            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var updateDto = UpdateDeckDto(
                deckName: "DeckToUpdate",
                commander: "NewCommander",
                cardList: "2 Test Card2\n"
            );

            var result = await uut.UpdateDeck("DeckToUpdate", updateDto);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var ok = result.Result as OkObjectResult;
            var updatedDeck = ok?.Value as DeckDTO;

            Assert.Multiple(() =>
            {
                Assert.That(updatedDeck, Is.Not.Null);
                Assert.That(updatedDeck.DeckCommander, Is.EqualTo("NewCommander"));
                Assert.That(updatedDeck.Cards.Count, Is.EqualTo(2));
                Assert.That(updatedDeck.Cards.All(c => c.Name == "Test Card2"), Is.True);
            });

            var dbDeck = await context.Decks.Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == "DeckToUpdate");
            Assert.That(dbDeck.Cards.Count, Is.EqualTo(2));
            Assert.That(dbDeck.DeckCommander, Is.EqualTo("NewCommander"));
        }

        [Test]
        public async Task UpdateDeck_DeckDoesNotExist_ReturnsNotFound()
        {
            var updateDto = UpdateDeckDto();
            var result = await uut.UpdateDeck("NonExistingDeck", updateDto);
            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidDeckNameOrDto_ReturnsBadRequest()
        {
            var updateDto = UpdateDeckDto();

            var result1 = await uut.UpdateDeck(null!, updateDto);
            Assert.That(result1.Result, Is.TypeOf<NotFoundResult>());

            var result2 = await uut.UpdateDeck("DeckName", null!);
            Assert.That(result2.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidCardNames_ReturnsBadRequestWithInvalidCards()
        {
            var player = await createPlayerAsync();
            var card1 = await createCardAsync("ValidCard");

            var deck = new Deck
            {
                DeckName = "DeckToUpdateCards",
                DeckCommander = "Test Commander",
                Player = player,
                Cards = new List<Card> { card1 }
            };
            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var updateDto = UpdateDeckDto(
                deckName: "DeckToUpdateCards",
                commander: "Test Commander",
                cardList: "1 ValidCard\n2 MissingCard\n"
            );

            var result = await uut.UpdateDeck("DeckToUpdateCards", updateDto);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());

            var badRequest = result.Result as BadRequestObjectResult;
            var value = badRequest?.Value as InvalidCardsResponse;

            Assert.That(value, Is.Not.Null);
            Assert.That(value.InvalidCards, Does.Contain("MissingCard"));
            Assert.That(value.InvalidCards, Does.Not.Contain("ValidCard"));
        }

        [Test]
        public async Task UpdateDeck_EmptyCardList_ClearsCards()
        {
            var player = await createPlayerAsync();
            var card = await createCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName = "DeckEmptyCards",
                DeckCommander = "Test Commander",
                Player = player,
                Cards = new List<Card> { card }
            };
            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var updateDto = UpdateDeckDto(
                deckName: "DeckEmptyCards",
                commander: "Test Commander",
                cardList: ""
            );

            var result = await uut.UpdateDeck("DeckEmptyCards", updateDto);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var updatedDeck = (result.Result as OkObjectResult)?.Value as DeckDTO;
            Assert.That(updatedDeck.Cards.Count, Is.EqualTo(0));

            var dbDeck = await context.Decks.Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.DeckName == "DeckEmptyCards");
            Assert.That(dbDeck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateDeck_InvalidCardLineFormat_ReturnsBadRequest()
        {
            await createPlayerAsync();
            var card = await createCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName = "DeckWithBadLine",
                DeckCommander = "Test Commander",
                Player = await createPlayerAsync(),
                Cards = new List<Card> { card }
            };
            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var updateDto = UpdateDeckDto(
                deckName: "DeckWithBadLine",
                commander: "Test Commander",
                cardList: "InvalidLineWithoutSpace"
            );

            var result = await uut.UpdateDeck("DeckWithBadLine", updateDto);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidQuantityInCardList_ReturnsBadRequest()
        {
            await createPlayerAsync();
            var card = await createCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName = "DeckWithBadQuantity",
                DeckCommander = "Test Commander",
                Player = await createPlayerAsync(),
                Cards = new List<Card> { card }
            };
            context.Decks.Add(deck);
            await context.SaveChangesAsync();

            var updateDto = UpdateDeckDto(
                deckName: "DeckWithBadQuantity",
                commander: "Test Commander",
                cardList: "X Card1"
            );

            var result = await uut.UpdateDeck("DeckWithBadQuantity", updateDto);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }


        // Helper functions

        private async Task<Player> createPlayerAsync(string username = "Test player")
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
            await context.SaveChangesAsync();
            return player;
        }

        private async Task<Card> createCardAsync(string name)
        {
            var card = new Card
            {
                Name = name,
                OracleText = "Test text",
                ImageUri = "http://Test.com"
            };

            context.Cards.Add(card);
            await context.SaveChangesAsync();
            return card;
        }

        private CreateDeckDTO createDeckDto(
            string playerName = "Test player",
            string deckName = "Test deck",
            string commander = "Test commander",
            string cardList = "1 Test card\n2 Test card2\n")
        {
            return new CreateDeckDTO
            {
                PlayerName = playerName,
                DeckName = deckName,
                Commander = commander,
                CardList = cardList
            };
        }

        private DeckDTO extractCreatedDto(ActionResult<DeckDTO> result)
        {
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created?.Value, Is.TypeOf<DeckDTO>());

            return created.Value as DeckDTO;
        }
    }
}
