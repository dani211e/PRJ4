using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Shared.Db.DTO.DeckDTO;

namespace UnitTests.Backend.Controllers
{
    public class TestDeckController : TestControllerBase
    {
        private DeckController uut = null!;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            uut = new DeckController(Context);
            SetControllerUser(uut, "test-api-user-id");
        }

        // CreateDeck

        [Test]
        public async Task CreateDeck_ValidInput_CreatesDeckWithCorrectCards()
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            await InsertCardAsync("Test card");

            var dto    = createDeckDto(cardList: "1 Test card\n", commandZone: ["Test commander"]);
            var result = await uut.CreateDeck(dto);
            var deck   = extractOkDto(result);

            Assert.Multiple(() =>
            {
                Assert.That(deck?.DeckName,              Is.EqualTo("Test deck"));
                Assert.That(deck?.CommandZone.Count,     Is.EqualTo(1));
                Assert.That(deck?.CommandZone[0].Name,   Is.EqualTo("Test commander"));
                Assert.That(deck?.Cards,                 Has.Count.EqualTo(1));
                Assert.That(deck?.Cards[0].Name,         Is.EqualTo("Test card"));
            });
        }

        [Test]
        public async Task CreateDeck_PlayerDoesNotExist_ReturnsNotFound()
        {
            var dto = createDeckDto(commandZone: ["Test commander"]);

            Assert.That((await uut.CreateDeck(dto)).Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateDeck_CardDoesNotExist_ReturnsBadRequest()
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            var dto = createDeckDto(cardList: "1 Missing card\n", commandZone: ["Test commander"]);

            Assert.That((await uut.CreateDeck(dto)).Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_CommandZoneCardDoesNotExist_ReturnsBadRequest()
        {
            await InsertPlayerAsync();
            var dto = createDeckDto(commandZone: ["Missing commander"]);

            Assert.That((await uut.CreateDeck(dto)).Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [TestCase("1 Test card\n", 1)]
        [TestCase("2 Test card\n", 2)]
        [TestCase("3 Test card\n", 3)]
        public async Task CreateDeck_ParsesCardQuantitiesCorrectly(string cardList, int expectedCount)
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            await InsertCardAsync("Test card");

            var deck = extractOkDto(await uut.CreateDeck(
                createDeckDto(cardList: cardList, commandZone: ["Test commander"])));

            Assert.That(deck?.Cards.Count, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task CreateDeck_InvalidCardLineFormat_ReturnsBadRequest()
        {
            await InsertPlayerAsync();

            Assert.That((await uut.CreateDeck(createDeckDto(cardList: "InvalidLineWithoutSpace"))).Result, 
                Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_InvalidQuantity_ReturnsBadRequest()
        {
            await InsertPlayerAsync();

            Assert.That((await uut.CreateDeck(createDeckDto(cardList: "X Test card"))).Result, 
                Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateDeck_DoubleFacedCardName_NormalizesAndFindsCard()
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            await InsertCardAsync("Huntmaster of the Fells");

            var deck = extractOkDto(await uut.CreateDeck(
                createDeckDto(
                    cardList:    "1 Huntmaster of the Fells // Ravager of the Fells\n",
                    commandZone: ["Test commander"])));

            Assert.That(deck?.Cards[0].Name, Is.EqualTo("Huntmaster of the Fells"));
        }

        [Test]
        public async Task CreateDeck_PersistsDeckInDatabase()
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            await InsertCardAsync("Test card");

            await uut.CreateDeck(createDeckDto(cardList: "1 Test card\n", commandZone: ["Test commander"]));

            var deck = await Context.Decks
                .Include(d => d.DeckCards)
                .Include(d => d.CommandZone)
                .FirstOrDefaultAsync();

            Assert.Multiple(() =>
            {
                Assert.That(deck,                                                  Is.Not.Null);
                Assert.That(deck!.DeckCards.Sum(dc => dc.Quantity), Is.EqualTo(1));
                Assert.That(deck.CommandZone.Count,                       Is.EqualTo(1));
            });
        }
        
        [Test]
        public async Task CreateDeck_CommandZoneCardAlsoInCardList_IsExcludedFromDeckCards()
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            await InsertCardAsync("Test card");

            var dto  = createDeckDto(
                cardList:    "1 Test commander\n1 Test card\n",
                commandZone: ["Test commander"]);
            var deck = extractOkDto(await uut.CreateDeck(dto));

            Assert.Multiple(() =>
            {
                Assert.That(deck?.CommandZone.Count,   Is.EqualTo(1));
                Assert.That(deck?.Cards.Count,         Is.EqualTo(1));
                Assert.That(deck?.Cards[0].Name,       Is.EqualTo("Test card"));
                Assert.That(deck?.Cards.Select(c => c.Name), Does.Not.Contain("Test commander"));
            });
        }
        
        [Test]
        public async Task CreateDeck_CommandZoneCardWithMultipleCopiesInCardList_RemovesOnlyOneCopy()
        {
            await InsertPlayerAsync();
            await InsertCardAsync("Test commander");
            await InsertCardAsync("Test card");

            var dto  = createDeckDto(
                cardList:    "3 Test commander\n1 Test card\n",
                commandZone: ["Test commander"]);
            var deck = extractOkDto(await uut.CreateDeck(dto));

            Assert.Multiple(() =>
            {
                Assert.That(deck?.CommandZone.Count,                                            Is.EqualTo(1));
                Assert.That(deck?.Cards.Count,                                                  Is.EqualTo(3)); 
                Assert.That(deck?.Cards.Count(c => c.Name == "Test commander"),   Is.EqualTo(2));
                Assert.That(deck?.Cards.Count(c => c.Name == "Test card"),        Is.EqualTo(1));
            });
        }

        // GetDeckById

        [Test]
        public async Task GetDeckById_ExistingDeck_ReturnsCorrectDeck()
        {
            var player    = await InsertPlayerAsync();
            var card      = await InsertCardAsync("Test card");
            var commander = await InsertCardAsync("Test commander");
            commander.AltFace = new CardFace
            {
                Name       = "Alt face",
                OracleText = "Alt text",
                ImageUri   = "http://altface.com",
            };
            card.RelatedCards = [new RelatedCard { Name = "Related card", ImageUri = "http://related.com" }];
            await Context.SaveChangesAsync();

            var deck = new Deck
            {
                DeckName    = "Test deck",
                Player      = player,
                CommandZone = [commander],
                DeckCards   = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var deckDto = (await uut.GetDeckById(deck.DeckId).ContinueWith(
                t => (t.Result.Result as OkObjectResult)?.Value as DeckDto));

            Assert.Multiple(() =>
            {
                Assert.That(deckDto,                                                Is.Not.Null);
                Assert.That(deckDto!.DeckName,                             Is.EqualTo("Test deck"));
                Assert.That(deckDto.CommandZone[0].Name,                   Is.EqualTo("Test commander"));
                Assert.That(deckDto.CommandZone[0].AltFace!.Name,          Is.EqualTo("Alt face"));
                Assert.That(deckDto.Cards.Count,                           Is.EqualTo(1));
                Assert.That(deckDto.Cards[0].Name,                         Is.EqualTo("Test card"));
                Assert.That(deckDto.Cards[0].RelatedCards[0].Name,         Is.EqualTo("Related card"));
            });
        }

        [Test]
        public async Task GetDeckById_DeckDoesNotExist_ReturnsNotFound()
        {
            Assert.That((await uut.GetDeckById(999)).Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetDeckById_CallerIsNotOwner_ReturnsForbid()
        {
            var player = await InsertPlayerAsync(apiUserId: "other-user-id");
            var deck   = new Deck { DeckName = "Other deck", Player = player, DeckCards = [] };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            Assert.That((await uut.GetDeckById(deck.DeckId)).Result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task GetDeckById_DeckWithMultipleCards_ExpandsQuantitiesCorrectly()
        {
            var player = await InsertPlayerAsync();
            var card1  = await InsertCardAsync("Test Card1");
            var card2  = await InsertCardAsync("Test Card2");

            var deck = new Deck
            {
                DeckName  = "MultiCardDeck",
                Player    = player,
                DeckCards =
                [
                    new DeckCard { Card = card1, Quantity = 1 },
                    new DeckCard { Card = card2, Quantity = 3 },
                ],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var deckDto = ((await uut.GetDeckById(deck.DeckId)).Result as OkObjectResult)?.Value as DeckDto;

            Assert.That(deckDto?.Cards.Count, Is.EqualTo(4));
        }

        // GetAllDecksByUsername

        [Test]
        public async Task GetAllDecksByUsername_ExistingPlayer_ReturnsDeckList()
        {
            var player    = await InsertPlayerAsync();
            var commander = await InsertCardAsync("Test commander");
            Context.Decks.Add(new Deck
            {
                DeckName    = "Deck One",
                Player      = player,
                CommandZone = [commander],
                DeckCards   = [],
            });
            await Context.SaveChangesAsync();

            var result = await uut.GetAllDecksByUsername("Test player");
            var decks  = (result.Result as OkObjectResult)?.Value as List<AllDecksDto>;

            Assert.Multiple(() =>
            {
                Assert.That(decks,                             Is.Not.Null);
                Assert.That(decks![0].DeckName,       Is.EqualTo("Deck One"));
                Assert.That(decks[0].DeckImageUri,    Is.EqualTo("http://Test.com"));
            });
        }

        [Test]
        public async Task GetAllDecksByUsername_PlayerDoesNotExist_ReturnsNotFound()
        {
            Assert.That((await uut.GetAllDecksByUsername("NonExistingPlayer")).Result,
                Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetAllDecksByUsername_CallerIsNotOwner_ReturnsForbid()
        {
            await InsertPlayerAsync(username: "Other player", apiUserId: "other-user-id");

            Assert.That((await uut.GetAllDecksByUsername("Other player")).Result,
                Is.TypeOf<ForbidResult>());
        }

        // DeleteDeckById

        [Test]
        public async Task DeleteDeckById_ExistingDeck_DeletesDeck()
        {
            var player = await InsertPlayerAsync();
            var deck   = new Deck { DeckName = "DeckToDelete", Player = player, DeckCards = [] };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            Assert.That(await uut.DeleteDeckByName(deck.DeckId), Is.TypeOf<NoContentResult>());
            Assert.That(await Context.Decks.FindAsync(deck.DeckId), Is.Null);
        }

        [Test]
        public async Task DeleteDeckById_DeckDoesNotExist_ReturnsNotFound()
        {
            Assert.That(await uut.DeleteDeckByName(999), Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteDeckById_CallerIsNotOwner_ReturnsForbid()
        {
            var player = await InsertPlayerAsync(apiUserId: "other-user-id");
            var deck   = new Deck { DeckName = "OthersDeck", Player = player, DeckCards = [] };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            Assert.That(await uut.DeleteDeckByName(deck.DeckId), Is.TypeOf<ForbidResult>());
        }

        // UpdateDeck

        [Test]
        public async Task UpdateDeck_ValidInput_UpdatesDeckAndCards()
        {
            var player = await InsertPlayerAsync();
            var card1  = await InsertCardAsync("Test Card1");
            var card2  = await InsertCardAsync("Test Card2");
            var card3  = await InsertCardAsync("Test Card3");

            var deck = new Deck
            {
                DeckName    = "OldName",
                Player      = player,
                CommandZone = [card1],
                DeckCards   = [new DeckCard { Card = card1, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.UpdateDeck(deck.DeckId, createUpdateDeckDto(
                deckName:    "NewName",
                cardList:    "2 Test Card2\n",
                commandZone: ["Test Card3"]
            ));

            Assert.That(result.Result, Is.TypeOf<NoContentResult>());

            var dbDeck = await Context.Decks
                .Include(d => d.DeckCards).ThenInclude(dc => dc.Card)
                .Include(d => d.CommandZone)
                .FirstOrDefaultAsync(d => d.DeckId == deck.DeckId);

            Assert.Multiple(() =>
            {
                Assert.That(dbDeck!.DeckName,                              Is.EqualTo("NewName"));
                Assert.That(dbDeck.DeckCards.Sum(dc => dc.Quantity), Is.EqualTo(2));
                Assert.That(dbDeck.CommandZone.First().Name,               Is.EqualTo("Test Card3"));
            });
        }

        [Test]
        public async Task UpdateDeck_DeckDoesNotExist_ReturnsNotFound()
        {
            Assert.That((await uut.UpdateDeck(999, createUpdateDeckDto())).Result,
                Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateDeck_NullDto_ReturnsBadRequest()
        {
            Assert.That((await uut.UpdateDeck(1, null!)).Result,
                Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task UpdateDeck_CallerIsNotOwner_ReturnsForbid()
        {
            var player = await InsertPlayerAsync(apiUserId: "other-user-id");
            var deck   = new Deck { DeckName = "OthersDeck", Player = player, DeckCards = [] };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            Assert.That((await uut.UpdateDeck(deck.DeckId, createUpdateDeckDto())).Result,
                Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task UpdateDeck_InvalidCardNames_ReturnsBadRequestWithInvalidCards()
        {
            var player = await InsertPlayerAsync();
            var card   = await InsertCardAsync("ValidCard");
            var deck   = new Deck
            {
                DeckName  = "Deck",
                Player    = player,
                DeckCards = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var value = ((await uut.UpdateDeck(deck.DeckId, createUpdateDeckDto(
                cardList: "1 ValidCard\n2 MissingCard\n"
            ))).Result as BadRequestObjectResult)?.Value as InvalidCardsResponse;

            Assert.That(value!.InvalidCards, Does.Contain("MissingCard"));
            Assert.That(value.InvalidCards,  Does.Not.Contain("ValidCard"));
        }
        
        [Test]
        public async Task UpdateDeck_InvalidCardLineFormat_ReturnsBadRequest()
        {
            var player = await InsertPlayerAsync();
            var card   = await InsertCardAsync("ValidCard");

            var deck = new Deck
            {
                DeckName  = "Deck",
                Player    = player,
                DeckCards = [new DeckCard { Card = card, Quantity = 1 }],
            };

            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.UpdateDeck(deck.DeckId,
                createUpdateDeckDto(cardList: "InvalidLineWithoutSpace"));

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }
        
        [Test]
        public async Task UpdateDeck_InvalidQuantity_ReturnsBadRequest()
        {
            var player = await InsertPlayerAsync();
            var card   = await InsertCardAsync("ValidCard");

            var deck = new Deck
            {
                DeckName  = "Deck",
                Player    = player,
                DeckCards = [new DeckCard { Card = card, Quantity = 1 }],
            };

            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            var result = await uut.UpdateDeck(deck.DeckId,
                createUpdateDeckDto(cardList: "X ValidCard"));

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }
        
        [Test]
        public async Task UpdateDeck_CommandZoneCardAlsoInCardList_IsExcludedFromDeckCards()
        {
            var player      = await InsertPlayerAsync();
            var commander   = await InsertCardAsync("Test commander");
            var card        = await InsertCardAsync("Test card");

            var deck = new Deck
            {
                DeckName  = "OldDeck",
                Player    = player,
                DeckCards = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            await uut.UpdateDeck(deck.DeckId, createUpdateDeckDto(
                cardList:    "1 Test commander\n1 Test card\n",
                commandZone: ["Test commander"]));

            var dbDeck = await Context.Decks
                .Include(d => d.DeckCards).ThenInclude(dc => dc.Card)
                .Include(d => d.CommandZone)
                .FirstOrDefaultAsync(d => d.DeckId == deck.DeckId);

            Assert.Multiple(() =>
            {
                Assert.That(dbDeck!.CommandZone.Count,                         Is.EqualTo(1));
                Assert.That(dbDeck.DeckCards.Sum(dc => dc.Quantity),           Is.EqualTo(1));
                Assert.That(dbDeck.DeckCards.Select(dc => dc.Card.Name), Does.Not.Contain("Test commander"));
            });
        }
        
        [Test]
        public async Task UpdateDeck_CommandZoneCardWithMultipleCopiesInCardList_RemovesOnlyOneCopy()
        {
            var player    = await InsertPlayerAsync();
            var commander = await InsertCardAsync("Test commander");
            var card      = await InsertCardAsync("Test card");

            var deck = new Deck
            {
                DeckName  = "OldDeck",
                Player    = player,
                DeckCards = [new DeckCard { Card = card, Quantity = 1 }],
            };
            Context.Decks.Add(deck);
            await Context.SaveChangesAsync();

            await uut.UpdateDeck(deck.DeckId, createUpdateDeckDto(
                cardList:    "3 Test commander\n1 Test card\n",
                commandZone: ["Test commander"]));

            var dbDeck = await Context.Decks
                .Include(d => d.DeckCards).ThenInclude(dc => dc.Card)
                .Include(d => d.CommandZone)
                .FirstOrDefaultAsync(d => d.DeckId == deck.DeckId);

            Assert.Multiple(() =>
            {
                Assert.That(dbDeck!.CommandZone.Count,                                               Is.EqualTo(1));
                Assert.That(dbDeck.DeckCards.Sum(dc => dc.Quantity),                           Is.EqualTo(3));
                Assert.That(dbDeck.DeckCards.First(dc => dc.Card.Name == "Test commander").Quantity, Is.EqualTo(2));
            });
        }

        // Helpers

        private static CreateDeckDto createDeckDto(
            string        deckName    = "Test deck",
            string        cardList    = "",
            List<string>? commandZone = null)
        {
            return new CreateDeckDto
            {
                DeckName    = deckName,
                CardList    = cardList,
                CommandZone = commandZone ?? [],
            };
        }

        private static UpdateDeckDto createUpdateDeckDto(
            string        deckName    = "Test deck",
            string        cardList    = "",
            List<string>? commandZone = null)
        {
            return new UpdateDeckDto
            {
                DeckName    = deckName,
                CardList    = cardList,
                CommandZone = commandZone ?? [],
            };
        }

        private static DeckDto? extractOkDto(ActionResult<DeckDto> result)
        {
            if (result.Result is OkObjectResult ok)
                return ok.Value as DeckDto;

            Assert.Fail($"Expected OkObjectResult but got {result.Result?.GetType().Name}");
            return null;
        }
    }
}