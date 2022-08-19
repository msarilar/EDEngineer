using System;
using System.Collections.Generic;

namespace EDEngineer.Models
{
    public interface IShoppingList : IEnumerable<Blueprint>
    {
        List<Tuple<Blueprint, int>> Composition { get; }
    }
}