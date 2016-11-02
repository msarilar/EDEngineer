using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Filters;
using EDEngineer.Properties;
using EDEngineer.Utils;

namespace EDEngineer.Models
{
    public class Blueprint : INotifyPropertyChanged
    {
        private bool favorite;
        private bool ignored;
        public string Type { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<string> Engineers { get; set; }
        public IReadOnlyCollection<BlueprintIngredient> Ingredients { get; set; }
        public int Grade { get; set; }

        public Blueprint(string type, string name, int grade, IReadOnlyCollection<BlueprintIngredient> ingredients, IReadOnlyCollection<string> engineers)
        {
            Type = type;
            Name = name;
            Grade = grade;
            Engineers = engineers;
            Ingredients = ingredients;

            foreach (var ingredient in Ingredients)
            {
                ingredient.Entry.PropertyChanged += (o, e) =>
                {
                    var extended = (PropertyChangedExtendedEventArgs<int>) e;

                    var progressBefore =
                        ComputeProgress(i => i.Entry.Name == ingredient.Entry.Name ? extended.OldValue : i.Entry.Count);

                    if (Math.Abs(progressBefore - Progress) > 0.1)
                    {
                        OnPropertyChanged(nameof(Progress));
                    }

                    var canCraftCountBefore =
                        ComputeCraftCount(i => i.Entry.Name == ingredient.Entry.Name ? extended.OldValue : i.Entry.Count);
                    
                    if (canCraftCountBefore != CanCraftCount)
                    {
                        OnPropertyChanged(nameof(CanCraftCount));
                        if (Favorite && canCraftCountBefore == 0 && CanCraftCount > 0)
                        {
                            FavoriteAvailable?.Invoke(this, EventArgs.Empty);
                        }
                    }
                };
            }
        }

        public bool Favorite
        {
            get { return favorite; }
            set
            {
                if (value == favorite) return;
                favorite = value;
                OnPropertyChanged();
            }
        }

        public bool Ignored
        {
            get { return ignored; }
            set
            {
                if (value == ignored) return;
                ignored = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get { return ComputeProgress(i => i.Entry.Count); }
        }

        public int CanCraftCount => ComputeCraftCount(i => i.Entry.Count);

        private int ComputeCraftCount(Func<BlueprintIngredient, int> countExtractor)
        {
            return Ingredients.Min(i => countExtractor(i)/i.Size);
        }

        private double ComputeProgress(Func<BlueprintIngredient, int> countExtractor)
        {
            return Ingredients.Sum(
                ingredient =>
                    Math.Min(1, countExtractor(ingredient)/(double) ingredient.Size)/Ingredients.Count)*100;
        }

        public double ProgressSorting => CanCraftCount + Progress;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler FavoriteAvailable;

        public override string ToString()
        {
            return $"G{Grade} [{Type}] {Name}";
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool SatisfyFilter(BlueprintFilter filter)
        {
            return filter.AppliesTo(this);
        }
    }
}