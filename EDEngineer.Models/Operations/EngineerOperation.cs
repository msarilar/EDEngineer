using System.Collections.Generic;
using EDEngineer.Models.Loadout;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models.Operations
{
    public class EngineerOperation : JournalOperation
    {
        public BlueprintCategory Category { get; }
        public string BlueprintName { get; }
        public string TechnicalType { get; }
        public string TechnicalSlot { get; }
        public string Engineer { get; }
        public int? Grade { get; }
        public string ExperimentalEffect { get; }

        public List<BlueprintIngredient> IngredientsConsumed { get; set; }
        public List<ModuleModifier> Modifiers { get; set; }

        public EngineerOperation(BlueprintCategory category,
            string blueprintName,
            string type,
            string slot,
            string engineer,
            int? grade,
            string experimentalEffect)
        {
            Category = category;
            BlueprintName = blueprintName;
            TechnicalType = type?.ToLowerInvariant();
            TechnicalSlot = slot?.ToLowerInvariant();
            Engineer = engineer;
            Grade = grade;
            ExperimentalEffect = experimentalEffect;
        }

        public override void Mutate(State state)
        {
            foreach (var ingredient in IngredientsConsumed)
            {
                state.Cargo.IncrementCargo(ingredient.Entry.Data.Name, -1 * ingredient.Size);
            }

            state.OnBlueprintCrafted(this);
        }
    }
}