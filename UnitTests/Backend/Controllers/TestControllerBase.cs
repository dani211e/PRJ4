using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB;
using NUnit.Framework;

namespace UnitTests.Backend.Controllers
{
    public class TestControllerBase
    {
        protected MTGContext Context;

        [SetUp]
        public virtual void Setup()
        {
            var options = new DbContextOptionsBuilder<MTGContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            Context = new MTGContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
