using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;

namespace UnitTests.Backend.Controllers
{
    public class TestDeckController : TestControllerBase
    {
        private DeckController uut;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            uut = new DeckController(Context);
            setControllerUser(uut, "test-api-user-id");
        }

        // CreateDeck

        [Test]
        public async Task CreateDeck_ValidInput_CreatesDeckWithCorrectCards()
        {
            await insertPlayerAsync();
            await insertCardAsync("Test commander");
            await insertCardAsync("Test card");

            var dto = createDeckDto(cardList: "1 Test commander\n1 Test card\n");

            var result = await uut.CreateDeck(dto);
            var deck = extractCreatedDto(result);

            Assert.Multiple(() =>
            {
                Assert.That(deck?.DeckName,      Is.EqualTo("Test deck"));
                Assert.That(deck?.DeckCommander, Is.EqualTo("Test commander"));
                Assert.That(deck?.Cards,         Has.Count.EqualTo(2));
                if (deck != null)
                {
                    Assert.That(deck.Cards.Count(c => c.Name == "Test commander"), Is.EqualTo(1));
                    Assert.That(deck.Cards.Count(c => c.Name == "Test card"),      Is.EqualTo(1));
                }
            });
        }

        [Test]
        public async Task CreateDeck_PlayerDoesNotExist_ReturnsNotFound()
        {
            await insertCardAsync("Test commander");
            var dto = createDeckDto(cardList: "1 Test commander\n");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateDeck_CardDoesNotExist_ReturnsBadRequest()
        {
            await insertPlayerAsync();
            var dto = createDeckDto(cardList: "1 Test commander\n1 Missing card\n");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [TestCase("1 Test commander\n", 1)]
        [TestCase("2 Test commander\n", 2)]
        [TestCase("3 Test commander\n", 3)]
        public async Task CreateDeck_ParsesCardQuantitiesCorrectly(string cardList, int expectedCount)
        {
            await insertPlayerAsync();
            await insertCardAsync("Test commander");

            var dto    = createDeckDto(cardList: cardList);
            var result = await uut.CreateDeck(dto);
            var deck   = extractCreatedDto(result);

            if (deck != null)
                Assert.That(deck.Cards.Count, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task CreateDeck_MultipleInvalidCards_ReturnsAllInvalidNames()
        {
            await insertPlayerAsync();
            var dto = createDeckDto(cardList: "1 Missing card\n2 Also missing\n");

            var result = await uut.CreateDeck(dto);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);

            var value = badRequest!.Value as InvalidCardsResponse;
            Assert.That(value,                   Is.Not.Null);
            Assert.That(value!.InvalidCards, Does.Contain("Missing card"));
            Assert.That(value.InvalidCards,  Does.Contain("Also missing"));
        }

        [Test]
        public async Task CreateDeck_SomeCardsInvalid_ReturnsOnlyInvalidNames()
        {
            await insertPlayerAsync();
            await insertCardAsync("Valid Card");

            var dto = createDeckDto(cardList: "1 Valid Card\n2 Missing Card\n");

            var result     = await uut.CreateDeck(dto);
            var badRequest = result.Result as BadRequestObjectResult;
            var value      = badRequest?.Value as InvalidCardsResponse;

            Assert.That(value,                       Is.Not.Null);
            Assert.That(value!.InvalidCards, Does.Contain("Missing Card"));
            Assert.That(value.InvalidCards,  Does.Not.Contain("Valid Card"));
        }

        [Test]
        public async Task CreateDeck_DuplicateCardLines_AreSummedCorrectly()
        {
            await insertPlayerAsync();
            await insertCardAsync("Test commander");

            var dto = createDeckDto(cardList: "1 Test commander\n2 Test commander\n");

            var result = await uut.CreateDeck(dto);
            var deck   = extractCreatedDto(result);

            if (deck != null)
                Assert.That(deck.Cards.Count(c => c.Name == "Test commander"), Is.EqualTo(3));
        }

        [Test]
        public async Task CreateDeck_CommanderNotInCardList_ReturnsBadRequest()
        {
            await insertPlayerAsync();
            await insertCardAsync("Test card");

            var dto = createDeckDto(cardList: "1 Test card\n");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_InvalidCardLineFormat_ReturnsBadRequest()
        {
            await insertPlayerAsync();
            var dto = createDeckDto(cardList: "InvalidLineWithoutSpace");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_InvalidQuantity_ReturnsBadRequest()
        {
            await insertPlayerAsync();
            var dto = createDeckDto(cardList: "X Test commander");

            var result = await uut.CreateDeck(dto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_PersistsDeckInDatabase()
        {
            await insertPlayerAsync();
            await insertCardAsync("Test commander");

            var dto = createDeckDto(cardList: "1 Test commander\n");

            await uut.CreateDeck(dto);

            var deck = await Context.Decks
                .Include(d => d.DeckCards)
                .FirstOrDefaultAsync();

            Assert.That(deck,                        Is.Not.Null);
            Assert.That(deck!.DeckCards.Sum(dc => dc.Quantity), Is.EqualTo(1));
        }

        // GetDeckById

        [Test]
        public async Task GetDeckById_ExistingDeck_ReturnsCorrectDeck()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test card");

            var deck = new Deck
            {
                DeckName      = "Test deck",
                CommanderName = "Test commander",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result  = await uut.GetDeckById(deck.DeckId);
            var ok      = result.Result as OkObjectResult;
            var deckDto = ok?.Value as DeckDto;

            Assert.Multiple(() =>
            {
                Assert.That(deckDto,               Is.Not.Null);
                Assert.That(deckDto!.DeckName,     Is.EqualTo("Test deck"));
                Assert.That(deckDto.DeckCommander, Is.EqualTo("Test commander"));
                Assert.That(deckDto.Cards.Count,   Is.EqualTo(1));
                Assert.That(deckDto.Cards[0].Name, Is.EqualTo("Test card"));
            });
        }

        [Test]
        public async Task GetDeckById_DeckDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.GetDeckById(999);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetDeckById_CallerIsNotOwner_ReturnsForbid()
        {
            var player = await insertPlayerAsync(apiUserId: "other-user-id");
            var deck = new Deck
            {
                DeckName      = "Other deck",
                CommanderName = "Commander",
                Player        = player,
                DeckCards     = [],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.GetDeckById(deck.DeckId);

            Assert.That(result.Result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task GetDeckById_DeckWithMultipleCards_ReturnsAllCards()
        {
            var player = await insertPlayerAsync();
            var card1  = await insertCardAsync("Test Card1");
            var card2  = await insertCardAsync("Test Card2");

            var deck = new Deck
            {
                DeckName      = "MultiCardDeck",
                CommanderName = "Test Card1",
                Player        = player,
                DeckCards     =
                [
                    new DeckCard { Card = card1, Quantity = 2 },
                    new DeckCard { Card = card2, Quantity = 1 },
                ],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result  = await uut.GetDeckById(deck.DeckId);
            var deckDto = (result.Result as OkObjectResult)?.Value as DeckDto;

            Assert.Multiple(() =>
            {
                Assert.That(deckDto,                                            Is.Not.Null);
                Assert.That(deckDto!.Cards.Count,                              Is.EqualTo(3));
                Assert.That(deckDto.Cards.Count(c => c.Name == "Test Card1"), Is.EqualTo(2));
                Assert.That(deckDto.Cards.Count(c => c.Name == "Test Card2"), Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetDeckById_CardWithRelatedCards_ReturnsRelatedCardDtos()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test card");
            card.RelatedCards = new List<RelatedCard>
            {
                new RelatedCard
                {
                    Name     = "Related card",
                    ImageUri = "http://related.com",
                }
            };
            await Context.SaveChangesAsync();

            var deck = new Deck
            {
                DeckName      = "Related Deck",
                CommanderName = "Test card",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result  = await uut.GetDeckById(deck.DeckId);
            var deckDto = (result.Result as OkObjectResult)?.Value as DeckDto;

            Assert.That(deckDto!.Cards[0].RelatedCards,        Has.Count.EqualTo(1));
            Assert.That(deckDto.Cards[0].RelatedCards[0].Name, Is.EqualTo("Related card"));
        }

        // GetAllDecksByUsername

        [Test]
        public async Task GetAllDecksByUsername_ExistingPlayer_ReturnsDeckList()
        {
            var player = await insertPlayerAsync();
            Context.Decks.Add(new Deck
            {
                DeckName      = "Deck One",
                CommanderName = "Commander",
                Player        = player,
                DeckCards     = [],
            });
            await Context.SaveChangesAsync();

            var result = await uut.GetAllDecksByUsername("Test player");
            var ok     = result.Result as OkObjectResult;
            var decks  = ok?.Value as List<AllDecksDto>;

            Assert.That(decks,             Is.Not.Null);
            Assert.That(decks!.Count,      Is.EqualTo(1));
            Assert.That(decks[0].DeckName, Is.EqualTo("Deck One"));
        }

        [Test]
        public async Task GetAllDecksByUsername_CommanderMatchesCard_ReturnsDeckImageUri()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test commander");

            Context.Decks.Add(new Deck
            {
                DeckName      = "Deck One",
                CommanderName = "Test commander",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            });
            await Context.SaveChangesAsync();

            var result = await uut.GetAllDecksByUsername("Test player");
            var decks  = (result.Result as OkObjectResult)?.Value as List<AllDecksDto>;

            Assert.That(decks![0].DeckImageUri, Is.EqualTo("http://Test.com"));
        }

        [Test]
        public async Task GetAllDecksByUsername_PlayerDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.GetAllDecksByUsername("NonExistingPlayer");

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetAllDecksByUsername_CallerIsNotOwner_ReturnsForbid()
        {
            await insertPlayerAsync(username: "Other player", apiUserId: "other-user-id");

            var result = await uut.GetAllDecksByUsername("Other player");

            Assert.That(result.Result, Is.TypeOf<ForbidResult>());
        }

        // DeleteDeckById

        [Test]
        public async Task DeleteDeckById_ExistingDeck_DeletesDeck()
        {
            var player = await insertPlayerAsync();
            var deck = new Deck
            {
                DeckName      = "DeckToDelete",
                CommanderName = "Test Commander",
                Player        = player,
                DeckCards     = [],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.DeleteDeckByName(deck.DeckId);
            Assert.That(result, Is.TypeOf<NoContentResult>());

            var deleted = await Context.Decks.FirstOrDefaultAsync(d => d.DeckId == deck.DeckId);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task DeleteDeckById_DeckDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.DeleteDeckByName(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteDeckById_CallerIsNotOwner_ReturnsForbid()
        {
            var player = await insertPlayerAsync(apiUserId: "other-user-id");
            var deck = new Deck
            {
                DeckName      = "OthersDeck",
                CommanderName = "Commander",
                Player        = player,
                DeckCards     = [],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.DeleteDeckByName(deck.DeckId);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        // UpdateDeck

        [Test]
        public async Task UpdateDeck_ExistingDeck_UpdatesDeckAndCards()
        {
            var player = await insertPlayerAsync();
            var card1  = await insertCardAsync("Test Card1");
            var card2  = await insertCardAsync("Test Card2");

            var deck = new Deck
            {
                DeckName      = "DeckToUpdate",
                CommanderName = "Test Card1",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card1, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var updateDto = createUpdateDeckDto(
                deckName:  "DeckToUpdate",
                commander: "Test Card2",
                cardList:  "2 Test Card2\n"
            );

            var result = await uut.UpdateDeck(deck.DeckId, updateDto);
            Assert.That(result.Result, Is.TypeOf<NoContentResult>());

            var dbDeck = await Context.Decks
                .Include(d => d.DeckCards)
                    .ThenInclude(dc => dc.Card)
                .FirstOrDefaultAsync(d => d.DeckId == deck.DeckId);

            Assert.Multiple(() =>
            {
                Assert.That(dbDeck,                                                             Is.Not.Null);
                Assert.That(dbDeck!.CommanderName,                                             Is.EqualTo("Test Card2"));
                Assert.That(dbDeck.DeckCards.Sum(dc => dc.Quantity),                           Is.EqualTo(2));
                Assert.That(dbDeck.DeckCards.All(dc => dc.Card.Name == "Test Card2"),          Is.True);
            });
        }

        [Test]
        public async Task UpdateDeck_DeckDoesNotExist_ReturnsNotFound()
        {
            var result = await uut.UpdateDeck(999, createUpdateDeckDto());

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateDeck_NullDto_ReturnsBadRequest()
        {
            var result = await uut.UpdateDeck(1, null!);

            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task UpdateDeck_CallerIsNotOwner_ReturnsForbid()
        {
            var player = await insertPlayerAsync(apiUserId: "other-user-id");
            var deck = new Deck
            {
                DeckName      = "OthersDeck",
                CommanderName = "Commander",
                Player        = player,
                DeckCards     = [],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.UpdateDeck(deck.DeckId, createUpdateDeckDto());

            Assert.That(result.Result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidCardNames_ReturnsBadRequestWithInvalidCards()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("ValidCard");

            var deck = new Deck
            {
                DeckName      = "DeckToUpdateCards",
                CommanderName = "ValidCard",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var updateDto = createUpdateDeckDto(
                deckName:  "DeckToUpdateCards",
                commander: "ValidCard",
                cardList:  "1 ValidCard\n2 MissingCard\n"
            );

            var result     = await uut.UpdateDeck(deck.DeckId, updateDto);
            var badRequest = result.Result as BadRequestObjectResult;
            var value      = badRequest?.Value as InvalidCardsResponse;

            Assert.That(value,                          Is.Not.Null);
            Assert.That(value!.InvalidCards, Does.Contain("MissingCard"));
            Assert.That(value.InvalidCards,  Does.Not.Contain("ValidCard"));
        }

        [Test]
        public async Task UpdateDeck_EmptyCardList_ReturnsBadRequest()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName      = "DeckEmptyCards",
                CommanderName = "Test Card",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var updateDto = createUpdateDeckDto(
                deckName:  "DeckEmptyCards",
                commander: "Test Card",
                cardList:  ""
            );

            var result = await uut.UpdateDeck(deck.DeckId, updateDto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateDeck_CommanderNotInCardList_ReturnsBadRequest()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName      = "DeckCommanderCheck",
                CommanderName = "Test Card",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var updateDto = createUpdateDeckDto(
                deckName:  "DeckCommanderCheck",
                commander: "New Commander",
                cardList:  "1 Test Card\n"
            );

            var result = await uut.UpdateDeck(deck.DeckId, updateDto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidCardLineFormat_ReturnsBadRequest()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName      = "DeckWithBadLine",
                CommanderName = "Test Card",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var updateDto = createUpdateDeckDto(
                deckName:  "DeckWithBadLine",
                commander: "Test Card",
                cardList:  "InvalidLineWithoutSpace"
            );

            var result = await uut.UpdateDeck(deck.DeckId, updateDto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidQuantityInCardList_ReturnsBadRequest()
        {
            var player = await insertPlayerAsync();
            var card   = await insertCardAsync("Test Card");

            var deck = new Deck
            {
                DeckName      = "DeckWithBadQuantity",
                CommanderName = "Test Card",
                Player        = player,
                DeckCards     = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var updateDto = createUpdateDeckDto(
                deckName:  "DeckWithBadQuantity",
                commander: "Test Card",
                cardList:  "X Test Card"
            );

            var result = await uut.UpdateDeck(deck.DeckId, updateDto);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        // Helpers

        private static void setControllerUser(ControllerBase controller, string apiUserId, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiUserId),
            };

            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var identity  = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private async Task<Player> insertPlayerAsync(
            string username  = "Test player",
            string apiUserId = "test-api-user-id")
        {
            var player = new Player
            {
                Username   = username,
                ApiUserId  = apiUserId,
                GamesWon   = 0,
                GamesLost  = 0,
                GamesDrawn = 0,
            };
            Context.Players.Add(player);
            await Context.SaveChangesAsync();
            return player;
        }

        private async Task<Card> insertCardAsync(string name)
        {
            var card = new Card
            {
                Name         = name,
                OracleText   = "Test text",
                ImageUri     = "http://Test.com",
                RelatedCards = new List<RelatedCard>(),
            };
            Context.Cards.Add(card);
            await Context.SaveChangesAsync();
            return card;
        }

        private static CreateDeckDto createDeckDto(
            string deckName  = "Test deck",
            string commander = "Test commander",
            string cardList  = "1 Test commander\n")
        {
            return new CreateDeckDto
            {
                DeckName  = deckName,
                Commander = commander,
                CardList  = cardList,
            };
        }

        private static UpdateDeckDto createUpdateDeckDto(
            string deckName  = "Test deck",
            string commander = "Test commander",
            string cardList  = "1 Test commander\n")
        {
            return new UpdateDeckDto
            {
                DeckName  = deckName,
                Commander = commander,
                CardList  = cardList,
            };
        }

        private static DeckDto? extractCreatedDto(ActionResult<DeckDto> result)
        {
            Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
            var created = result.Result as CreatedAtActionResult;
            Assert.That(created?.Value, Is.TypeOf<DeckDto>());
            return created!.Value as DeckDto;
        }
    }
}