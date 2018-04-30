using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models;
using EDEngineer.Models.State;
using EDEngineer.Utils.System;
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
            entries = JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson());
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

        [TestCase(1, 3)]
        [TestCase(2, 9)]
        [TestCase(3, 27)]
        [TestCase(4, 81)]
        public void Simple_downgrade_trade(int rank, int expected)
        {
            var group = Group.Alloys;
            var alloys = entries.Where(e => e.Group == group)
                                .OrderBy(e => e.Rarity)
                                .ToList();

            var firstGrade = alloys[rank];
            var secondGrade = new Entry(alloys[0]);

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
    }
}
