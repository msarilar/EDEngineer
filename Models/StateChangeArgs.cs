namespace EDEngineer.Models
{
    public class StateChangeArgs
    {
        public StateChangeArgs(Kind kind, string name, int newValue)
        {
            Kind = kind;
            Name = name;
            NewValue = newValue;
        }

        public Kind Kind { get; }
        public string Name { get; }
        public int NewValue { get; }
    }
}