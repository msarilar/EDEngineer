namespace EDEngineer.Models.MaterialTrading
{
    public class MaterialTrade
    {
        public MaterialTrade(Entry traded, Entry needed, int tradedNeeded, int missing, bool willBeEnough, int alreadyNeeded)
        {
            Traded = traded;
            Needed = needed;
            TradedNeeded = tradedNeeded;
            Missing = missing;
            WillBeEnough = willBeEnough;
            AlreadyNeeded = alreadyNeeded;
        }

        public Entry Traded { get; }
        public Entry Needed { get; }
        public int TradedNeeded { get; }
        public int Missing { get; }
        public bool WillBeEnough { get; }
        public int AlreadyNeeded { get; }

        public decimal? Consumption => Traded.Count == AlreadyNeeded ? (decimal?) null : (TradedNeeded / (decimal) (Traded.Count - AlreadyNeeded)) * 100m;
    }
}