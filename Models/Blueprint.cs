using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Filters;
using EDEngineer.Properties;

namespace EDEngineer.Models
{
    public class Blueprint : INotifyPropertyChanged
    {
        private bool favorite;
        private bool ignored;
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> Engineers { get; set; }
        public List<BlueprintIngredient> Ingredients { get; set; }

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

        public int Grade { get; set; }

        public double Progress
        {
            get
            {
                return Ingredients.Sum(
                    ingredient =>
                        Math.Min(1, ingredient.Current/(double) ingredient.Size)/Ingredients.Count)*100;
            }
        }

        public int CanCraftCount => Ingredients.Min(i => i.Current/i.Size);

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

        public void OnStateChanged(object sender, StateChangeArgs e)
        {
            if (e.NewValue < 0)
            {
                return;
            }

            foreach (var ingredient in Ingredients.Where(ingredient => ingredient.Name == e.Name))
            {
                var progressBefore = Progress;
                var canCraftCountBefore = CanCraftCount;

                ingredient.Current = e.NewValue;

                if (Math.Abs(progressBefore - Progress) > 0.01)
                {
                    OnPropertyChanged(nameof(Progress));
                }

                if (canCraftCountBefore != CanCraftCount)
                {
                    OnPropertyChanged(nameof(CanCraftCount));
                    if (Favorite && canCraftCountBefore == 0 && CanCraftCount > 0)
                    {
                        FavoriteAvailable?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool SatisfyFilter(BlueprintFilter filter)
        {
            return filter.AppliesTo(this);
        }
    }
}