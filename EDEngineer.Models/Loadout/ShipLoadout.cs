using System.Collections.Generic;

namespace EDEngineer.Models.Loadout
{
    public class ShipLoadout
    {
        public ShipLoadout(string ship, string shipName, string shipIdent, int? shipValue, int? modulesValue, int? rebuy, List<ShipModule> modules)
        {
            Ship = string.IsNullOrEmpty(ship) ? null : ship;
            ShipName = string.IsNullOrEmpty(shipName) ? null : shipName;
            ShipIdent = string.IsNullOrEmpty(shipIdent) ? null : shipIdent;
            ShipValue = shipValue;
            ModulesValue = modulesValue;
            Rebuy = rebuy;
            Modules = modules;
        }

        public string Ship { get; }
        public string ShipName { get; }
        public string ShipIdent { get; }
        public int? ShipValue { get; }
        public int? ModulesValue { get; }
        public int? Rebuy { get; }
        public List<ShipModule> Modules { get; }
    }
}