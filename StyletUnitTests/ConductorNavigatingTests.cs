﻿using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ConductorNavigatingTests
    {
        private Conductor<IScreen>.Collections.Navigation conductor;

        [SetUp]
        public void SetUp()
        {
            this.conductor = new Conductor<IScreen>.Collections.Navigation();
        }

        [Test]
        public void ActiveItemIsNullBeforeAnyItemsActivated()
        {
            Assert.IsNull(this.conductor.ActiveItem);
            Assert.That(this.conductor.GetChildren(), Is.EquivalentTo(new IScreen[] { null }));
        }

        [Test]
        public void InitialActivateSetsItemAsActiveItem()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            Assert.AreEqual(screen.Object, this.conductor.ActiveItem);
        }

        [Test]
        public void InitialActivateDoesNotActivateItemIfConductorIsNotActive()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Never);
        }

        [Test]
        public void InitialActivateActivatesItemIfConductorIsActive()
        {
            ((IActivate)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void ActivatesActiveItemWhenActivated()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Never);

            ((IActivate)this.conductor).Activate();
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void DeactivatesActiveItemWhenDeactivated()
        {
            ((IActivate)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            ((IDeactivate)this.conductor).Deactivate();
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void ActivateDeactivatesPreviousItemIfConductorIsActiveAndPreviousItemCanClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen1.Object);
            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen2.Object);
            screen1.Verify(x => x.Deactivate());
        }

        [Test]
        public void ActivatingCurrentScreenReactivatesScreen()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Exactly(2));
            screen.Verify(x => x.Close(), Times.Never);
        }

        [Test]
        public void CloseItemDoesNothingIfToldToDeactiveInactiveItem()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.CloseItem(screen2.Object);

            screen1.Verify(x => x.Close(), Times.Never);
            screen2.Verify(x => x.Activate(), Times.Never);
        }

        [Test]
        public void DeactiveDoesNotChangeActiveItem()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.DeactivateItem(screen.Object);

            screen.Verify(x => x.Deactivate());
            Assert.AreEqual(this.conductor.ActiveItem, screen.Object);
        }

        [Test]
        public void ActivateSetsConductorAsItemsParent()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.VerifySet(x => x.Parent = this.conductor);
        }

        [Test]
        public void CloseRemovesItemsParent()
        {
            var screen = new Mock<IScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            screen.Setup(x => x.Parent).Returns(this.conductor);
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.VerifySet(x => x.Parent = null);
        }
        
        [Test]
        public void CanCloseReturnsTrueIfNoActiveItem()
        {
            Assert.IsTrue(this.conductor.CanCloseAsync().Result);
        }

        [Test]
        public void CanCloseReturnsAllItemsCanClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);
            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(false));
            Assert.IsFalse(this.conductor.CanCloseAsync().Result);
        }

        [Test]
        public void DeactivatingActiveItemGoesBack()
        {
            ((IActivate)this.conductor).Activate();
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();

            this.conductor.ActivateItem(screen1.Object);
            screen1.Verify(x => x.Activate());

            this.conductor.ActivateItem(screen2.Object);
            screen2.Verify(x => x.Activate());

            this.conductor.DeactivateItem(screen2.Object);
            screen2.Verify(x => x.Deactivate(), Times.Once);
            screen1.Verify(x => x.Activate(), Times.Once);
        }

        [Test]
        public void ClearClosesAllItemsExceptCurrent()
        {
            ((IActivate)this.conductor).Activate();
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);
            this.conductor.Clear();

            Assert.AreEqual(screen2.Object, this.conductor.ActiveItem);

            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.GoBack();
            Assert.IsNull(this.conductor.ActiveItem);
        }
    }
}