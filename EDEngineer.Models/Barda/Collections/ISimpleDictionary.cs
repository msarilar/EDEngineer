namespace EDEngineer.Models.Barda.Collections
{
    public interface ISimpleDictionary<in TKey, TValue>
    {
        TValue this[TKey key]
        {
            get;
            set;
        }
    }
}