using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.Controllers;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Backend
{
    public class TokenControllerTest
    {
        private TokenController _controller;
        private MTGContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new MTGContext(options);
            _controller = new TokenController(_context);
        }

        [Test]
        public async Task GetTokenByName_ExistingToken_ReturnsToken()
        {
            var relatedCards = new List<RelatedCard>
            {
                new RelatedCard {Name = "Germ"}
            };

            var controller = new TokenController(relatedCards);
            var result = await controller.GetTokenByName("Germ");

            Assert.Equals(result,controller);
        }
    }
}
