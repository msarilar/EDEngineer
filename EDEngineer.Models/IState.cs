using System.Collections.Generic;
using EDEngineer.Models.Utils.Collections;

namespace EDEngineer.Models
{
    public interface IState
    {
        SortedObservableCounter Cargo { get; }
        string System { get; set; }
        void IncrementCargo(string name, int change);
        void OnBlueprintCrafted(BlueprintCategory category, string technicalName, List<BlueprintIngredient> ingredientsConsumed);
    }
}