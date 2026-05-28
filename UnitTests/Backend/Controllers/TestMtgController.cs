using Microsoft.AspNetCore.Mvc;
using MTG_Emulator.Backend.Controllers;

namespace UnitTests.Backend.Controllers
{
    public class TestMtgController : TestControllerBase
    {
        private class ConcreteMtgController : MtgController { }

        private ConcreteMtgController uut = null!;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            uut = new ConcreteMtgController();
        }

        [Test]
        public void IsOwnerOrAdmin_CallerIsOwner_ReturnsTrue()
        {
            SetControllerUser(uut, "user-id");
            Assert.That(uut.IsOwnerOrAdmin("user-id"), Is.True);
        }

        [Test]
        public void IsOwnerOrAdmin_CallerIsAdmin_ReturnsTrue()
        {
            SetControllerUser(uut, "other-id", isAdmin: true);
            Assert.That(uut.IsOwnerOrAdmin("user-id"), Is.True);
        }

        [Test]
        public void IsOwnerOrAdmin_CallerIsNeitherOwnerNorAdmin_ReturnsFalse()
        {
            SetControllerUser(uut, "other-id");
            Assert.That(uut.IsOwnerOrAdmin("user-id"), Is.False);
        }
    }
}