namespace EDEngineer.Models.Filters
{
    public interface ILabelledFilter
    {
        string Label { get; }
        bool Magic { get; }
    }
}