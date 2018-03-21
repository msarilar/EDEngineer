using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Models
{
    public class ShoppingListBlock : INotifyPropertyChanged
    {
        private bool highlighted;

        public ShoppingListBlock(string label, string tooltip, List<Tuple<Blueprint, int>> composition, BlueprintCategory category)
        {
            Label = label;
            Tooltip = tooltip;
            Composition = composition;
            Category = category;

            foreach (var blueprint in Composition)
            {
                blueprint.Item1.PropertyChanged += (o, e) =>
                                                   {
                                                       if (e.PropertyName == "ShoppingListHighlighted")
                                                       {
                                                           this.Highlighted = ((Blueprint)o).ShoppingListHighlighted;
                                                       }
                                                   };
            }
        }

        public string Label { get; }
        public string Tooltip { get; }
        public List<Tuple<Blueprint, int>> Composition { get; }
        public BlueprintCategory Category { get; }
        public int Width => Composition.Count > 1 ? 300 : 96;

        public bool Highlighted
        {
            get => highlighted;
            set
            {
                highlighted = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}