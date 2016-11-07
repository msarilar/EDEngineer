using EDEngineer.Models.Operations;
using NUnit.Framework;

namespace EDEngineerTests.Utils
{
    [TestFixture]
    public class JournalEntryConverterTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TestCase("", ExpectedResult = null)]
        public JournalOperation HandleManualUserChange(string json)
        {
            return null;
        }
    }
}
