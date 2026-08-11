using System;
using AegisRTS.Core.Time;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class GameClockTests
    {
        [Test]
        public void Advance_RespectsSpeedPauseAndResume()
        {
            var clock = new GameClock();

            clock.Advance(1d);
            clock.SetSpeed(2d);
            clock.Advance(1.5d);
            clock.Pause();
            clock.Advance(5d);

            Assert.That(clock.TotalSeconds, Is.EqualTo(4d));
            Assert.That(clock.TotalUnscaledSeconds, Is.EqualTo(7.5d));
            Assert.That(clock.DeltaSeconds, Is.Zero);
            Assert.That(clock.TickCount, Is.EqualTo(3));

            clock.Resume();
            clock.Advance(0.5d);

            Assert.That(clock.DeltaSeconds, Is.EqualTo(1d));
            Assert.That(clock.TotalSeconds, Is.EqualTo(5d));
        }

        [TestCase(0d)]
        [TestCase(-1d)]
        [TestCase(double.PositiveInfinity)]
        public void SetSpeed_RejectsInvalidValues(double speed)
        {
            var clock = new GameClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetSpeed(speed));
        }
    }
}
