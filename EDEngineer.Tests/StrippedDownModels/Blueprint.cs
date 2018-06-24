using System;
using System.Collections.Generic;

namespace EDEngineer.Tests.StrippedDownModels
{
    public class Blueprint
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<string> Engineers { get; set; }
        public IReadOnlyCollection<BlueprintIngredient> Ingredients { get; set; }
        public IReadOnlyCollection<BlueprintEffect> Effects { get; set; }
        public int? Grade { get; set; }
        public Guid? CoriolisGuid { get; set; }
    }
}
