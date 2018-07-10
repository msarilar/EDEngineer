using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.Loadout;

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

        public override void Mutate(State.State state)
        {
            foreach (var ingredient in IngredientsConsumed)
            {
                state.IncrementCargo(ingredient.Entry.Data.Name, -1 * ingredient.Size);
            }

            state.OnBlueprintCrafted(this);
        }

        public override Dictionary<string, int> Changes =>
            IngredientsConsumed.ToDictionary(i => i.Entry.Data.Name, i => -1 * i.Size);
    }
}