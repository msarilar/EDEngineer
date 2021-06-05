using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class MaterialTradeOperation : JournalOperation
    {
        private Dictionary<string, int> _changes = new Dictionary<string, int>();

        public void AddIngredient(string ingredient, int count)
		{
            if (!_changes.ContainsKey(ingredient))
                _changes.Add(ingredient, 0);
            _changes[ingredient] += count;
        }

        public void RemoveIngredient(string ingredient, int count)
        {
            if (!_changes.ContainsKey(ingredient))
                _changes.Add(ingredient, 0);
            _changes[ingredient] -= count;
        }
        
        public override void Mutate(State.State state)
        {
			foreach (var ingredient in _changes)
            {
                state.IncrementCargo(ingredient.Key, ingredient.Value);
            }
        }

        public override Dictionary<string, int> Changes => _changes;
    }
}