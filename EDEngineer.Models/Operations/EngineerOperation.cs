using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class EngineerOperation : JournalOperation
    {
        public string TechnicalModuleName { get; }

        public List<BlueprintIngredient> IngredientsConsumed { get; set; }

        public EngineerOperation(string technicalModuleName)
        {
            TechnicalModuleName = technicalModuleName;
        }

        public override void Mutate(State state)
        {
            foreach (var ingredient in IngredientsConsumed)
            {
                state.IncrementCargo(ingredient.Entry.Data.Name, -1 * ingredient.Size);
            }

            if(!string.IsNullOrEmpty(TechnicalModuleName))
            {
                state.OnBlueprintCrafted(TechnicalModuleName, IngredientsConsumed);
            }
        }
    }
}