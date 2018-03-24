using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Utils.Collections;

namespace EDEngineer.Models
{
    [DebuggerDisplay("{Label} {Composition.Count}")]
    public class ShoppingListBlock : INotifyPropertyChanged
    {
        private bool highlighted;
        private bool showAllGrades;

        public ShoppingListBlock(string label, string tooltip, List<Tuple<Blueprint, int>> composition, BlueprintCategory category, bool showAllGrades)
        {
            Label = label;
            Tooltip = tooltip;
            Composition = new SortedObservableCollection<Tuple<Blueprint, int>>((b1, b2) => b1.Item1.Grade.GetValueOrDefault().CompareTo(b2.Item1.Grade.GetValueOrDefault()));
            foreach (var blueprint in composition)
            {
                Composition.Add(blueprint);
            }
            ShowAllGrades = showAllGrades;

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
        public SortedObservableCollection<Tuple<Blueprint, int>> Composition { get; }
        public BlueprintCategory Category { get; }
        public int Width => Composition.Count > 3 ? 300 : Composition.Count > 1 ? 198 : 96;

        public List<Tuple<Blueprint, int>> HiddenBlueprints { get; } = new List<Tuple<Blueprint, int>>();

        public int BlueprintsWithCount => Composition.Count(c => c.Item2 > 0);

        public bool Highlighted
        {
            get => highlighted;
            set
            {
                highlighted = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAllGrades
        {
            get => showAllGrades;
            set
            {
                showAllGrades = value;
                OnPropertyChanged();
                if (showAllGrades)
                {
                    foreach (var blueprint in HiddenBlueprints)
                    {
                        Composition.Add(blueprint);
                    }

                    HiddenBlueprints.Clear();
                }
                else
                {
                    foreach (var blueprint in Composition.ToList())
                    {
                        if (blueprint.Item2 == 0)
                        {
                            HiddenBlueprints.Add(blueprint);
                            Composition.Remove(blueprint);
                        }
                    }
                }

                OnPropertyChanged(nameof(Width));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}