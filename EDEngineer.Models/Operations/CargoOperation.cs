namespace EDEngineer.Models.Operations
{
    public class CargoOperation : JournalOperation
    {
        public string CommodityName { get; set; }

        public int Size { get; set; }

        public override void Mutate(IState state)
        {
            state.IncrementCargo(CommodityName, Size);
        }
    }
}