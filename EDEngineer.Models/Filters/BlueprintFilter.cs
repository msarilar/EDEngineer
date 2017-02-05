namespace EDEngineer.Models.Filters
{
    public abstract class BlueprintFilter : Filter<Blueprint>
    {
        protected BlueprintFilter(string uniqueName) : base(uniqueName)
        {
        }
    }
}