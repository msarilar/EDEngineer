using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Barda;
using EDEngineer.Models.Filters;
using Newtonsoft.Json;

namespace EDEngineer.Models
{
    public class Blueprint : INotifyPropertyChanged
    {
        private readonly ILanguage language;
        private bool favorite;
        private bool ignored;
        public string Type { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<string> Engineers { get; set; }
        public IReadOnlyCollection<BlueprintIngredient> Ingredients { get; set; }
        public int Grade { get; set; }

        [JsonIgnore]
        public bool Synthesis => Engineers.FirstOrDefault() == "@Synthesis";

        [JsonIgnore]
        public string TranslatedType => language.Translate(Type);

        [JsonIgnore]
        public string TranslatedName => language.Translate(Name);

        public Blueprint(ILanguage language, string type, string name, int grade, IReadOnlyCollection<BlueprintIngredient> ingredients, IReadOnlyCollection<string> engineers)
        {
            this.language = language;
            Type = type;
            Name = name;
            Grade = grade;
            Engineers = engineers;
            Ingredients = ingredients;

            foreach (var ingredient in Ingredients)
            {
                ingredient.Entry.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName != "Count")
                    {
                        return;
                    }

                    var extended = (PropertyChangedExtendedEventArgs<int>) e;

                    var progressBefore =
                        ComputeProgress(i => Math.Max(0, i.Entry.Data.Name == ingredient.Entry.Data.Name ? extended.OldValue : i.Entry.Count));

                    if (Math.Abs(progressBefore - Progress) > 0.1)
                    {
                        OnPropertyChanged(nameof(Progress));
                    }

                    var canCraftCountBefore =
                        ComputeCraftCount(i => Math.Max(0, i.Entry.Data.Name == ingredient.Entry.Data.Name ? extended.OldValue : i.Entry.Count));
                    
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

        [JsonIgnore]
        public bool Favorite
        {
            get { return favorite; }
            set
            {
                if (value == favorite) return;
                favorite = value;

                if (Synthesis)
                {
                    Ingredients.Select(ingredient => ingredient.Entry).ToList().ForEach(entry => entry.SynthesisFavoriteCount += value ? 1 : -1);
                }
                else
                {
                    Ingredients.Select(ingredient => ingredient.Entry).ToList().ForEach(entry => entry.FavoriteCount += value ? 1 : -1);
                }

                OnPropertyChanged();
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public double Progress
        {
            get { return ComputeProgress(i => Math.Max(0, i.Entry.Count)); }
        }

        [JsonIgnore]
        public int CanCraftCount => ComputeCraftCount(i => Math.Max(0, i.Entry.Count));

        [JsonIgnore]
        public bool JustMissingCommodities
            => CanCraftCount == 0 &&
                Ingredients.All(
                    i =>
                        i.Size <= i.Entry.Count && i.Entry.Data.Kind != Kind.Commodity || // not commodity and just enough or more stock available
                        i.Size > i.Entry.Count && i.Entry.Data.Kind == Kind.Commodity); // commodity and not enough stock

        private int ComputeCraftCount(Func<BlueprintIngredient, int> countExtractor)
        {
            return Ingredients.Any() ? Ingredients.Min(i => countExtractor(i)/i.Size) : 0;
        }

        private double ComputeProgress(Func<BlueprintIngredient, int> countExtractor)
        {
            return Ingredients.Sum(
                ingredient =>
                    Math.Min(1, countExtractor(ingredient)/(double) ingredient.Size)/Ingredients.Count)*100;
        }

        [JsonIgnore]
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
    }
}