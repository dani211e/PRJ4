using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.AuthenticationDTO;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Backend.Controllers
{
    public class TestAuthenticationController : TestControllerBase
    {
        private AuthenticationController uut;
        private Mock<UserManager<ApiUser>> userManagerMock;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            userManagerMock = new Mock<UserManager<ApiUser>>(
                Mock.Of<IUserStore<ApiUser>>(),
                null!, null!, null!, null!, null!, null!, null!, null!
            );

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JWT:SigningKey", "super-secret-test-signing-key-that-is-long-enough" },
                    { "JWT:Issuer",    "test-issuer"   },
                    { "JWT:Audience",  "test-audience" },
                }!)
                .Build();

            uut = new AuthenticationController(configuration, userManagerMock.Object, Context)
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

        // Register

        [Test]
        public async Task Register_PlayerRole_CreatesUserAndPlayerProfile()
        {
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApiUser>(), "Password1!"))
                .Callback<ApiUser, string>((u, _) => u.Id = "new-user-id")
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApiUser>(), Roles.Player))
                .ReturnsAsync(IdentityResult.Success);

            var result = await uut.Register(new RegisterDto
            {
                Username = "TestUser",
                Email    = "test@example.com",
                Password = "Password1!",
                Role     = Roles.Player,
            });

            Assert.That(result, Is.TypeOf<OkObjectResult>());

            var player = Context.Players.Local.FirstOrDefault(p => p.Username == "TestUser");
            Assert.That(player, Is.Not.Null);
        }

        [Test]
        public async Task Register_AdminRole_UnauthenticatedCaller_ReturnsForbid()
        {
            var result = await uut.Register(new RegisterDto
            {
                Username = "AdminUser",
                Email    = "admin@example.com",
                Password = "Password1!",
                Role     = Roles.Admin,
            });

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task Register_AdminRole_CallerIsAdmin_CreatesAdminWithoutPlayerProfile()
        {
            SetControllerUser(uut, "caller-id", isAdmin: true);

            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApiUser>(), "Password1!"))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApiUser>(), Roles.Admin))
                .ReturnsAsync(IdentityResult.Success);

            var result = await uut.Register(new RegisterDto
            {
                Username = "AdminUser",
                Email    = "admin@example.com",
                Password = "Password1!",
                Role     = Roles.Admin,
            });

            Assert.That(result, Is.TypeOf<OkObjectResult>());

            var player = Context.Players.Local.FirstOrDefault(p => p.Username == "AdminUser");
            Assert.That(player, Is.Null);
        }

        [Test]
        public async Task Register_UserManagerFails_ReturnsBadRequest()
        {
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApiUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short." }));

            var result = await uut.Register(new RegisterDto
            {
                Username = "TestUser",
                Email    = "test@example.com",
                Password = "bad",
                Role     = Roles.Player,
            });

            var bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.Value as IEnumerable<string>, Contains.Item("Password too short."));
        }

        // Login

        [Test]
        public async Task Login_ValidCredentials_ReturnsTokenInBody()
        {
            var user = new ApiUser { Id = "user-id", UserName = "TestUser", Email = "test@example.com" };

            userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, "Password1!")).ReturnsAsync(true);
            userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { Roles.Player });

            var result = await uut.Login(new LoginDto { Email = user.Email, Password = "Password1!" });
            var ok     = result as OkObjectResult;

            Assert.That(ok, Is.Not.Null);

            var token = ok!.Value?.GetType().GetProperty(nameof(LoginResponseDto.Token))?.GetValue(ok.Value) as string;
            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            userManagerMock
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApiUser?)null);

            var result = await uut.Login(new LoginDto { Email = "nobody@example.com", Password = "x" });

            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var user = new ApiUser { Id = "user-id", UserName = "TestUser", Email = "test@example.com" };

            userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

            var result = await uut.Login(new LoginDto { Email = user.Email, Password = "wrong" });

            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
        }

        // ResetPassword

        [Test]
        public async Task ResetPassword_PasswordMismatch_ReturnsBadRequest()
        {
            SetControllerUser(uut, "user-id");

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                NewPassword     = "Password1!",
                ConfirmPassword = "Different1!",
            });

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPassword_NoUserIdClaim_ReturnsUnauthorized()
        {
            uut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                NewPassword     = "Password1!",
                ConfirmPassword = "Password1!",
            });

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public async Task ResetPassword_UserNotFound_ReturnsNotFound()
        {
            SetControllerUser(uut, "ghost-id");

            userManagerMock
                .Setup(m => m.FindByIdAsync("ghost-id"))
                .ReturnsAsync((ApiUser?)null);

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                NewPassword     = "Password1!",
                ConfirmPassword = "Password1!",
            });

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task ResetPassword_CallerIsNotOwner_ReturnsForbid()
        {
            SetControllerUser(uut, "caller-id");

            userManagerMock
                .Setup(m => m.FindByIdAsync("caller-id"))
                .ReturnsAsync(new ApiUser { Id = "other-user-id" });

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                NewPassword     = "Password1!",
                ConfirmPassword = "Password1!",
            });

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task ResetPassword_ValidRequest_ReturnsNoContent()
        {
            SetControllerUser(uut, "user-id");

            var user = new ApiUser { Id = "user-id", UserName = "TestUser", Email = "test@example.com" };

            userManagerMock.Setup(m => m.FindByIdAsync("user-id")).ReturnsAsync(user);
            userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            userManagerMock.Setup(m => m.ResetPasswordAsync(user, "reset-token", "Password1!")).ReturnsAsync(IdentityResult.Success);

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                NewPassword     = "Password1!",
                ConfirmPassword = "Password1!",
            });

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task ResetPassword_IdentityFails_ReturnsBadRequest()
        {
            SetControllerUser(uut, "user-id");

            var user = new ApiUser { Id = "user-id", UserName = "TestUser", Email = "test@example.com" };

            userManagerMock.Setup(m => m.FindByIdAsync("user-id")).ReturnsAsync(user);
            userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            userManagerMock
                .Setup(m => m.ResetPasswordAsync(It.IsAny<ApiUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                NewPassword     = "weak",
                ConfirmPassword = "weak",
            });

            var bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.Value as IEnumerable<string>, Contains.Item("Password too weak."));
        }
        
        [Test]
        public async Task ResetPassword_AdminResetsOtherUser_ReturnsNoContent()
        {
            SetControllerUser(uut, "admin-id", isAdmin: true);

            var target = new ApiUser { Id = "target-id", UserName = "TargetUser", Email = "target@example.com" };

            userManagerMock.Setup(m => m.FindByNameAsync("TargetUser")).ReturnsAsync(target);
            userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(target)).ReturnsAsync("reset-token");
            userManagerMock.Setup(m => m.ResetPasswordAsync(target, "reset-token", "Password1!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await uut.ResetPassword(new ResetPasswordDto
            {
                TargetUsername  = "TargetUser",
                NewPassword     = "Password1!",
                ConfirmPassword = "Password1!",
            });

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }
        
        // DeleteAccount

        [Test]
        public async Task DeleteAccount_UserNotFound_ReturnsNotFound()
        {
            SetControllerUser(uut, "ghost-id");

            userManagerMock
                .Setup(m => m.FindByIdAsync("ghost-id"))
                .ReturnsAsync((ApiUser?)null);

            var result = await uut.DeleteAccount();

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteAccount_CallerIsNotOwner_ReturnsForbid()
        {
            SetControllerUser(uut, "caller-id");

            userManagerMock
                .Setup(m => m.FindByIdAsync("caller-id"))
                .ReturnsAsync(new ApiUser { Id = "other-user-id" });

            var result = await uut.DeleteAccount();

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task DeleteAccount_PlayerExists_RemovesPlayerAndDeletesUser_ReturnsNoContent()
        {
            SetControllerUser(uut, "user-id");

            var user = new ApiUser
            {
                Id = "user-id",
                UserName = "TestUser",
                Email = "test@example.com"
            };

            var player = new Player
            {
                Username = "TestUser",
                ApiUserId = "user-id"
            };

            Context.Players.Add(player);
            await Context.SaveChangesAsync();

            userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            userManagerMock
                .Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var result = await uut.DeleteAccount();

            Assert.That(result, Is.TypeOf<NoContentResult>());

            var deletedPlayer = Context.Players
                .FirstOrDefault(p => p.ApiUserId == "user-id");

            Assert.That(deletedPlayer, Is.Null);

            userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
        }

        [Test]
        public async Task DeleteAccount_NoPlayerProfile_DeletesUser_ReturnsNoContent()
        {
            SetControllerUser(uut, "user-id");

            var user = new ApiUser
            {
                Id = "user-id",
                UserName = "TestUser",
                Email = "test@example.com"
            };

            userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            userManagerMock
                .Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var result = await uut.DeleteAccount();

            Assert.That(result, Is.TypeOf<NoContentResult>());

            userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
        }

        [Test]
        public async Task DeleteAccount_UserDeletionFails_ReturnsBadRequest()
        {
            SetControllerUser(uut, "user-id");

            var user = new ApiUser
            {
                Id       = "user-id",
                UserName = "TestUser",
                Email    = "test@example.com"
            };

            userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            userManagerMock
                .Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed." }));

            var result = await uut.DeleteAccount();

            var bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.Value as IEnumerable<string>, Contains.Item("Delete failed."));
        }
        
        [Test]
        public async Task DeleteAccount_AdminDeletesOtherUser_ReturnsNoContent()
        {
            SetControllerUser(uut, "admin-id", isAdmin: true);

            var target = new ApiUser { Id = "target-id", UserName = "TargetUser", Email = "target@example.com" };

            var player = new Player { Username = "TargetUser", ApiUserId = "target-id" };
            Context.Players.Add(player);
            await Context.SaveChangesAsync();

            userManagerMock.Setup(m => m.FindByNameAsync("TargetUser")).ReturnsAsync(target);
            userManagerMock.Setup(m => m.DeleteAsync(target)).ReturnsAsync(IdentityResult.Success);

            var result = await uut.DeleteAccount(targetUsername: "TargetUser");

            Assert.That(result, Is.TypeOf<NoContentResult>());

            var deletedPlayer = await Context.Players.FirstOrDefaultAsync(p => p.ApiUserId == "target-id");
            Assert.That(deletedPlayer, Is.Null);

            userManagerMock.Verify(m => m.DeleteAsync(target), Times.Once);
        }
    }
}