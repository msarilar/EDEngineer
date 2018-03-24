namespace EDEngineer.Models
{
    public class BlueprintEffect
    {
        public BlueprintEffect(string property, string effect, bool isGood)
        {
            Property = property;
            Effect = effect;
            IsGood = isGood;
        }

        public string Property { get; }
        public string Effect { get; }
        public bool IsGood { get; }
    }
}