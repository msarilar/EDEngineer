using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models;
using EDEngineer.Models.MaterialTrading;
using EDEngineer.Models.State;
using EDEngineer.Models.Utils;
using Moq;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;

namespace EDEngineer.Tests
{
    [TestFixture]
    public class MaterialTraderTests
    {
        private StateCargo cargo;
        private List<EntryData> entries;

        [SetUp]
        public void Setup()
        {
            entries = JsonConvert.DeserializeObject<List<EntryData>>(IO.GetEntryDatasJson());
            cargo = new StateCargo(entries, Mock.Of<ILanguage>(), StateCargo.COUNT_COMPARER);
        }

        [TestCase(1, 6)]
        [TestCase(2, 36)]
        [TestCase(3, 216)]
        [TestCase(4, 1296)]
        public void Simple_upgrade_trade(int rank, int expected)
        {
            var group = Group.Alloys;
            var alloys = entries.Where(e => e.Group == group)
                                .OrderBy(e => e.Rarity)
                                .ToList();

            var firstGrade = alloys[0];
            var secondGrade = new Entry(alloys[rank]);

            cargo.IncrementCargo(firstGrade.Name, expected * 2);

            var missingIngredients = new Dictionary<Entry, int>
            {
                [secondGrade] = 1
            };
            
            var trades = MaterialTrader.FindPossibleTrades(cargo, missingIngredients, new Dictionary<EntryData, int>()).ToList();

            Check.That(trades.Count).IsEqualTo(1);

            var trade = trades[0];

            Check.That(trade.Traded.Data).IsEqualTo(firstGrade);
            Check.That(trade.TradedNeeded).IsEqualTo(expected);
        }

        [TestCase(1, 1, 1, true)]
        [TestCase(1, 2, 4, true)]
        [TestCase(2, 1, 9, true)]
        [TestCase(2, 2, 10, true)]
        [TestCase(3, 1, 27, true)]
        [TestCase(3, 2, 28, true)]
        [TestCase(4, 1, 81, true)]
        [TestCase(4, 2, 82, true)]
        [TestCase(2, 1, 1, true)]
        [TestCase(3, 1, 1, true)]
        [TestCase(4, 1, 1, true)]
        [TestCase(0, 60, 10, false)]
        [TestCase(1, 2, 1, false)]
        [TestCase(1, 4, 2, false)]
        [TestCase(2, 2, 3, false)]
        [TestCase(2, 4, 4, false)]
        [TestCase(2, 4, 6, false)]
        [TestCase(2, 6, 7, false)]
        [TestCase(4, 2, 10, false)]
        [TestCase(4, 2, 7, false)]
        [TestCase(4, 2, 6, false)]
        [TestCase(3, 2, 6, false)]
        [TestCase(3, 2, 7, false)]
        [TestCase(3, 4, 10, false)]
        public void Simple_downgrade_trade(int rank, int expected, int missing, bool sameGroup)
        {
            var group = Group.Alloys;
            var alloys = entries.Where(e => e.Group == group)
                                .OrderBy(e => e.Rarity)
                                .ToList();

            var firstGrade = alloys[rank];
            Entry secondGrade;
            if (sameGroup)
            {
                secondGrade = new Entry(alloys[0]);
            }
            else
            {
                secondGrade = new Entry(entries.First(e => e.Group != group && e.Rarity.Rank() == 1 && e.Subkind == firstGrade.Subkind && e.Kind == firstGrade.Kind));
            }

            cargo.IncrementCargo(firstGrade.Name, expected * 2);

            var missingIngredients = new Dictionary<Entry, int>
            {
                [secondGrade] = missing
            };

            var trades = MaterialTrader.FindPossibleTrades(cargo, missingIngredients, new Dictionary<EntryData, int>()).ToList();

            Check.That(trades.Count).IsEqualTo(1);

            var trade = trades[0];

            Check.That(trade.Traded.Data).IsEqualTo(firstGrade);
            Check.That(trade.TradedNeeded).IsEqualTo(expected);
        }
    }
}
