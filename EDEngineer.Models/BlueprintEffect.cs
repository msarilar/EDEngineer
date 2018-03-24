namespace EDEngineer.Models
{
    public class BlueprintEffect
    {
        public BlueprintEffect(string property, string effect)
        {
            Property = property;
            Effect = effect;
        }

        public string Property { get; }
        public string Effect { get; }
    }
}