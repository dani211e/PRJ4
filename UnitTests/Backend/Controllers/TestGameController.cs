using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.GameDTO;

namespace UnitTests.Backend.Controllers
{
    public class TestGameController : TestControllerBase
    {
        private GameController uut;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            uut = new GameController(Context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity())
                    }
                }
            };
        }
        //CreateGame
        [Test] 
        public async Task CreateGame_PlayerNotFound_ReturnsNotFound()
        {
            SetControllerUser(uut, "ghost-id");

            var result = await uut.CreateGame(new CreateGameDto { MaxPlayers = 2 });

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateGame_PlayerAlreadyInGame_ReturnsConflict()
        {
            var player = await InsertPlayerAsync(apiUserId: "ghost-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 2,
                CurrentPlayers = 1,
                HostName = player.Username,
                PlayerNames = new List<string> { player.Username },
                Status = "Waiting"
            };
            
            Context.Games.Add(game);
            await Context.SaveChangesAsync();
            
            player.CurrentGameId = game.GameId;
            await Context.SaveChangesAsync();
            
            SetControllerUser(uut, "ghost-id"); 
            var result = await uut.CreateGame(new CreateGameDto { MaxPlayers = 2 });
            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
        }

        [Test]
        public async Task CreateGame_ValidRequest_ReturnsOkWithGameCode()
        {
            await InsertPlayerAsync(apiUserId: "ghost-id");
            SetControllerUser(uut, "ghost-id");
            
            var result = await uut.CreateGame(new CreateGameDto { MaxPlayers = 4 });
            
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            
            var response = ok!.Value as GameResponseDto;
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.Success, Is.True);
            Assert.That(response.GameCode,Has.Length.EqualTo(6));
            Assert.That(response.MaxPlayers,Is.EqualTo(4));
            Assert.That(response.CurrentPlayers,Is.EqualTo(1));
        }

        [Test]
        public async Task CreateGame_ValidRequest_SetsPlayerCurrentGameId()
        {
            await InsertPlayerAsync(apiUserId: "ghost-id");
            SetControllerUser(uut, "ghost-id");
            
            await uut.CreateGame(new CreateGameDto { MaxPlayers = 4 });
            
            var updatedPlayer = await Context.Players.FirstAsync(p => p.ApiUserId == "ghost-id");
            Assert.That(updatedPlayer.CurrentGameId, Is.Not.Null);
        }

        [Test]
        public async Task CreateGame_ValidRequest_GameCodeAddedToHashSet()
        {
            await InsertPlayerAsync(apiUserId: "ghost-id");
            SetControllerUser(uut, "ghost-id");
            
            var result = await uut.CreateGame(new CreateGameDto { MaxPlayers = 4 });
            
            var ok =  result.Result as OkObjectResult;
            var response = ok!.Value as GameResponseDto;
            
            var game = await Context.Games.FirstAsync(g => g.GameCode == response!.GameCode);
            Assert.That(game, Is.Not.Null);
        }
        
        //JoinGame
        [Test]
        public async Task JoinGame_PlayerNot_ReturnsNotFound()
        {
            SetControllerUser(uut, "ghost-id");
            
            var result = await uut.JoinGame(new JoinGameDto { GameCode = "ABCDEF" });
            
            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task JoinGame_PlayerAlreadyInGame_ReturnsConflict()
        {
            var player = await InsertPlayerAsync(apiUserId: "ghost-id");
            
            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "other",
                PlayerNames = new List<string> { "other" },
                Status = "Waiting"
            };
            
            Context.Games.Add(game);
            await Context.SaveChangesAsync();
            
            player.CurrentGameId = game.GameId;
            await Context.SaveChangesAsync();
            
            SetControllerUser(uut, "ghost-id");
            
            var result = await uut.JoinGame(new JoinGameDto { GameCode = "ABCDEF" });
            
            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
        }

        [Test]
        public async Task JoinGame_GameNotFound_ReturnsNotFound()
        {
            await InsertPlayerAsync(apiUserId: "ghost-id");
            SetControllerUser(uut, "ghost-id");
            
            var result = await uut.JoinGame(new JoinGameDto { GameCode = "XXXXXX" });
            
            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task JoinGame_GameFull_ReturnsConflict()
        {
            await InsertPlayerAsync(username: "Host",   apiUserId: "host-id");
            await InsertPlayerAsync(username: "Joiner", apiUserId: "joiner-id");
            
            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 2,
                CurrentPlayers = 2,
                HostName = "Host",
                PlayerNames = new List<string> { "Host", "Other" },
                Status = "Waiting"
            };
            
            Context.Games.Add(game);
            await Context.SaveChangesAsync();
            
            SetControllerUser(uut, "joiner-id");
            
            var result = await uut.JoinGame(new JoinGameDto{ GameCode = "ABCDEF" });
            
            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
        }

        [Test]
        public async Task JoinGame_PlayerIsHost_ReturnsConflict()
        {
            await InsertPlayerAsync(username: "Host", apiUserId: "host-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "host-id");

            var result = await uut.JoinGame(new JoinGameDto { GameCode = "ABCDEF" });

            Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());    
        }

        [Test]
        public async Task JoinGame_ValidRequest_ReturnsOk()
        {
            await InsertPlayerAsync(username: "Host",   apiUserId: "host-id");
            await InsertPlayerAsync(username: "Joiner", apiUserId: "joiner-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };
            
            Context.Games.Add(game);
            await  Context.SaveChangesAsync();
            
            SetControllerUser(uut, "joiner-id");
            var result = await uut.JoinGame(new JoinGameDto { GameCode = "ABCDEF" });

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);

            var response = ok!.Value as GameResponseDto;
            Assert.That(response!.Success,        Is.True);
            Assert.That(response.CurrentPlayers,  Is.EqualTo(2));
        }

        [Test]
        public async Task JoinGame_LastPlayerJoins_SetsStatusToInProgress()
        {
            await InsertPlayerAsync(username: "Host",   apiUserId: "host-id");
            await InsertPlayerAsync(username: "Joiner", apiUserId: "joiner-id");
            
            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 2,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };
            
            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "joiner-id");

            await uut.JoinGame(new JoinGameDto { GameCode = "ABCDEF" });

            var updatedGame = await Context.Games.FirstAsync(g => g.GameCode == "ABCDEF");
            Assert.That(updatedGame.Status, Is.EqualTo("InProgress"));
        }

        [Test]
        public async Task JoinGame_LastPlayerJoins_RandomizesPlayerOrder()
        {
            await InsertPlayerAsync(username: "Host",   apiUserId: "host-id");
            await InsertPlayerAsync(username: "Joiner", apiUserId: "joiner-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 2,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };
            
            Context.Games.Add(game);
            await Context.SaveChangesAsync();
            
            SetControllerUser(uut, "joiner-id");
            
            await uut.JoinGame(new JoinGameDto { GameCode = "ABCDEF" });

            var updatedGame = await Context.Games.FirstAsync(g => g.GameCode == "ABCDEF");
            Assert.That(updatedGame.PlayerNames, Is.EquivalentTo(new List<string> { "Host", "Joiner" }));
        }
        
        //LeaveGame
        [Test]
        public async Task LeaveGame_PlayerNotFound_ReturnsNotFound()
        {
            SetControllerUser(uut, "ghost-id");

            var result = await uut.LeaveGame("ABCDEF");

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }
        
        [Test]
        public async Task LeaveGame_GameNotFound_ReturnsNotFound()
        {
            var player = await InsertPlayerAsync(apiUserId: "user-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Test player",
                PlayerNames = new List<string> { "Test player" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            player.CurrentGameId = game.GameId;
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            var result = await uut.LeaveGame("XXXXXX");

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }
        [Test]
        public async Task LeaveGame_PlayerNotInGame_ReturnsBadRequest()
        {
            await InsertPlayerAsync(username: "Player", apiUserId: "user-id");
            var otherPlayer = await InsertPlayerAsync(username: "Other", apiUserId: "other-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Other",
                PlayerNames = new List<string> { "Other" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            otherPlayer.CurrentGameId = game.GameId;
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            var result = await uut.LeaveGame("ABCDEF");

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
        
        [Test]
        public async Task LeaveGame_ValidRequest_ReturnsNoContent()
        {
            var player = await InsertPlayerAsync(username: "Host", apiUserId: "user-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            player.CurrentGameId = game.GameId;
            game.Players.Add(player);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            var result = await uut.LeaveGame("ABCDEF");

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task LeaveGame_ValidRequest_ClearsPlayerCurrentGameId()
        {
            var player = await InsertPlayerAsync(username: "Host", apiUserId: "user-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            player.CurrentGameId = game.GameId;
            game.Players.Add(player);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            await uut.LeaveGame("ABCDEF");

            var updatedPlayer = await Context.Players.FirstAsync(p => p.ApiUserId == "user-id");
            Assert.That(updatedPlayer.CurrentGameId, Is.Null);
        }
        
        [Test]
        public async Task LeaveGame_LastPlayerLeaves_DeletesGame()
        {
            var player = await InsertPlayerAsync(username: "Host", apiUserId: "user-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            player.CurrentGameId = game.GameId;
            game.Players.Add(player);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            await uut.LeaveGame("ABCDEF");

            var deletedGame = await Context.Games
                .FirstOrDefaultAsync(g => g.GameCode == "ABCDEF");
            Assert.That(deletedGame, Is.Null);
        }
        
        [Test]
        public async Task LeaveGame_PlayerInDifferentGame_ReturnsBadRequest()
        {
            var player = await InsertPlayerAsync(username: "Player", apiUserId: "user-id");

            var ownedGame = new Game
            {
                GameCode = "GAME01",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Player",
                PlayerNames = new List<string> { "Player" },
                Status = "Waiting"
            };
            var otherGame = new Game
            {
                GameCode = "GAME02",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "SomeoneElse",
                PlayerNames = new List<string> { "SomeoneElse" },
                Status = "Waiting"
            };

            Context.Games.AddRange(ownedGame, otherGame);
            await Context.SaveChangesAsync();

            player.CurrentGameId = ownedGame.GameId;
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            var result = await uut.LeaveGame(otherGame.GameCode);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
        
        //GetGame
        [Test]
        public async Task GetGame_GameNotFound_ReturnsNotFound()
        {
            SetControllerUser(uut, "user-id");

            var result = await uut.GetGame("XXXXXX");

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetGame_ValidRequest_ReturnsOkWithGameState()
        {
            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "user-id");

            var result = await uut.GetGame("ABCDEF");

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);

            var response = ok!.Value as GameResponseDto;
            Assert.That(response!.Success,        Is.True);
            Assert.That(response.GameCode,        Is.EqualTo("ABCDEF"));
            Assert.That(response.MaxPlayers,      Is.EqualTo(4));
            Assert.That(response.CurrentPlayers,  Is.EqualTo(1));
            Assert.That(response.Message,         Is.EqualTo("Waiting"));
            Assert.That(response.PlayerNames,     Is.EquivalentTo(new List<string> { "Host" }));
            Assert.That(response.CurrentPlayerName, Is.EqualTo("Host"));
        }
        
        //DeleteGame
        [Test]
        public async Task DeleteGame_GameNotFound_ReturnsNotFound()
        {
            SetControllerUser(uut, "admin-id", isAdmin: true);

            var result = await uut.DeleteGame("XXXXXX");

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteGame_ValidRequest_ReturnsNoContent()
        {
            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "admin-id", isAdmin: true);

            var result = await uut.DeleteGame("ABCDEF");

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteGame_ValidRequest_RemovesGameFromDatabase()
        {
            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "admin-id", isAdmin: true);

            await uut.DeleteGame("ABCDEF");

            var deletedGame = await Context.Games
                .FirstOrDefaultAsync(g => g.GameCode == "ABCDEF");
            Assert.That(deletedGame, Is.Null);
        }

        [Test]
        public async Task DeleteGame_ValidRequest_ClearsPlayerCurrentGameId()
        {
            var player = await InsertPlayerAsync(username: "Host", apiUserId: "host-id");

            var game = new Game
            {
                GameCode = "ABCDEF",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                HostName = "Host",
                PlayerNames = new List<string> { "Host" },
                Status = "Waiting"
            };

            Context.Games.Add(game);
            await Context.SaveChangesAsync();

            player.CurrentGameId = game.GameId;
            game.Players.Add(player);
            await Context.SaveChangesAsync();

            SetControllerUser(uut, "admin-id", isAdmin: true);

            await uut.DeleteGame("ABCDEF");

            var updatedPlayer = await Context.Players
                .FirstAsync(p => p.ApiUserId == "host-id");
            Assert.That(updatedPlayer.CurrentGameId, Is.Null);
        }
        
    }

}

