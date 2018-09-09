using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class EngineerContributionOperation : JournalOperation
    {
        public string Engineer { get; }
        public JournalOperation Operation { get; }

        public EngineerContributionOperation(string engineer)
        {
            Engineer = engineer;
            engineersProgressOperation.TryGetValue(Engineer, out var operation);
            Operation = operation;
        }

        public override void Mutate(State.State state)
        {
            Operation?.Mutate(state);
        }

        public override Dictionary<string, int> Changes => Operation?.Changes ?? new Dictionary<string, int>();

        private static readonly Dictionary<string, JournalOperation> engineersProgressOperation = new Dictionary<string, JournalOperation>
        {
            ["Elivra Martuuk"] = new NoOperation(),
            ["The Dweller"] = new NoOperation(),
            ["Liz Ryder"] = new NoOperation(),
            ["Felicity Farseer"] = new NoOperation(),
            ["Tod McQuinn"] = new NoOperation(),
            ["Zacariah Nemo"] = new NoOperation(),
            ["Lei Cheung"] = new NoOperation(),
            ["Hera Tani"] = new NoOperation(),
            ["Juri Ishmaak"] = new NoOperation(),
            ["Selene Jean"] = new NoOperation(),
            ["Marco Qwent"] = new CargoOperation { CommodityName = "Modular Terminals", Size = -25, JournalEvent = JournalEvent.EngineerContribution },
            ["Ram Tah"] = new DataOperation { DataName = "Classified Scan Databanks", Size = -50, JournalEvent = JournalEvent.EngineerContribution },
            ["Broo Tarquin"] = new NoOperation(),
            ["Colonel Bris Dekker"] = new NoOperation(),
            ["Didi Vatermann"] = new NoOperation(),
            ["Professor Palin"] = new MaterialOperation {  MaterialName = "Sensor Fragment" , Size = -25, JournalEvent = JournalEvent.EngineerContribution },
            ["Lori Jameson"] = new NoOperation(),
            ["Tiana Fortune"] = new DataOperation { DataName = "Decoded Emission Data", Size = -50, JournalEvent = JournalEvent.EngineerContribution },
            ["The Sarge"] = new DataOperation { DataName = "Aberrant Shield Pattern Analysis", Size = -50, JournalEvent = JournalEvent.EngineerContribution },
            ["Bill Turner"] = new CargoOperation { CommodityName = "Bromellite", Size = -50, JournalEvent = JournalEvent.EngineerContribution }
            ["Petra Olmanova"] = new CargoOperation { CommodityName = "Progenitor Cells", Size = -200, JournalEvent = JournalEvent.EngineerContribution }
            ["Marsha Hicks"] = new CargoOperation { CommodityName = "Osmium", Size = -10, JournalEvent = JournalEvent.EngineerContribution }
            ["Etienne Dorn"] = new CargoOperation { CommodityName = "Occupied Escape Pod", Size = -25, JournalEvent = JournalEvent.EngineerContribution }
            ["Mel Brandon"] = new NoOperation(),
        };
    }
}
