using System;
using Moq;
using NFluent;
using NUnit.Framework;

namespace EDEngineer.Tests
{
    [TestFixture]
    public class LogParsingTests
    {
        [Test]
        public void SampleTest()
        {
            var flag = false;

            // mocking interfaces:
            var disposable = new Mock<IDisposable>();
            disposable.Setup(d => d.Dispose()).Callback(() => flag = true);

            disposable.Object.Dispose();

            // fluent check:
            Check.That(flag).IsTrue();
        }
    }
}
