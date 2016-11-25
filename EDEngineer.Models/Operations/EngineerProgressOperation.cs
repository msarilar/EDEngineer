using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class EngineerProgressOperation : JournalOperation
    {
        public string Engineer { get; }
        public JournalOperation Operation { get; }

        public EngineerProgressOperation(string engineer)
        {
            Engineer = engineer;
            JournalOperation operation;
            engineersProgressOperation.TryGetValue(Engineer, out operation);
            Operation = operation;
        }

        public override void Mutate(State state)
        {
            Operation?.Mutate(state);
        }

        private static readonly Dictionary<string, JournalOperation> engineersProgressOperation = new Dictionary<string, JournalOperation>()
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
            ["Marco Qwent"] = new CargoOperation() { CommodityName = "Modular Terminals", Size = -25, JournalEvent = JournalEvent.EngineerProgress },
            ["Ram Tah"] = new DataOperation() { DataName = "Classified Scan Databanks", Size = -50, JournalEvent = JournalEvent.EngineerProgress },
            ["Broo Tarquin"] = new NoOperation(),
            ["Colonel Bris Dekker"] = new NoOperation(),
            ["Didi Vatermann"] = new NoOperation(),
            ["Professor Palin"] = new MaterialOperation() {  MaterialName = "Unknown Fragment" , Size = -25, JournalEvent = JournalEvent.EngineerProgress },
            ["Lori Jameson"] = new NoOperation(),
            ["Tiana Fortune"] = new DataOperation() { DataName = "Decoded Emission Data", Size = -50, JournalEvent = JournalEvent.EngineerProgress },
            ["The Sarge"] = new DataOperation() { DataName = "Aberrant Shield Pattern Analysis", Size = -50, JournalEvent = JournalEvent.EngineerProgress },
            ["Bill Turner"] = new CargoOperation() { CommodityName = "Bromellite", Size = -50, JournalEvent = JournalEvent.EngineerProgress }
        };
    }
}