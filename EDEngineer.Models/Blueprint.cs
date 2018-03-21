using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EDEngineer.Models.Utils;
using Newtonsoft.Json;

namespace EDEngineer.Models
{
    public class Blueprint : INotifyPropertyChanged
    {
        private static readonly Dictionary<string, string> technicalTypes = new Dictionary<string, string>
        {
            ["Frame Shift Drive"] = "hyperdrive",
            ["Heat Sink Launcher"] = "heatsinklauncher",
            ["Collector Limpet Controller"] = "dronecontrol_collection",
            ["Prospector Limpet Controller"] = "dronecontrol", // _propsection?
            ["Fuel Transfer Limpet Controller"] = "dronecontrol", // _fuel?
            ["Hatch Breaker Limpet Controller"] = "dronecontrol", // _hatch?
            ["Thrusters"] = "engine",
            ["Pulse Laser"] = "pulselaser",
            ["Beam Laser"] = "beamlaser",
            ["Shield Booster"] = "shieldbooster",
            ["Shield Generator"] = "shieldgenerator",
            ["Sensors"] = "sensors",
            ["Plasma Accelerator"] = "plasmaaccelerator",
            ["Life Support"] = "lifesupport",
            ["Hull Reinforcement Package"] = "hullreinforcement",
            ["Power Distributor"] = "powerdistributor",
            ["Power Plant"] = "powerplant",
            ["Multi-cannon"] = "multicannon",
            ["Kill Warrant Scanner"] = "crimescanner",
            ["Rail Gun"] = "railgun",
            ["Burst Laser"] = "pulselaserburst"
        };

        private readonly ILanguage language;
        private bool favorite;
        private bool ignored;
        private int shoppingListCount;
        private bool shoppingListHighlighted;
        public string Type { get; }

        [JsonIgnore]
        public string TechnicalType => technicalTypes.TryGetValue(Type, out var result) ? result : Type;

        public string ShortenedType
        {
            get
            {
                switch (Type)
                {
                    case "Electronic Countermeasure":
                        return "ECM";
                    case "Hull Reinforcement Package":
                        return "Hull";
                    case "Frame Shift Drive Interdictor":
                        return "FSD Interdictor";
                    case "Prospector Limpet Controller":
                        return "Prospector LC";
                    case "Fuel Transfer Limpet Controller":
                        return "Fuel Transfer LC";
                    case "Hatch Breaker Limpet Controller":
                        return "Hatch Breaker LC";
                    case "Collector Limpet Controller":
                        return "Collector LC";
                    case "Auto Field-Maintenance Unit":
                        return "AFMU";
                    default:
                        return Type;
                }
            }
        }
        public string BlueprintName { get; }
        public IReadOnlyCollection<string> Engineers { get; }
        public IReadOnlyCollection<BlueprintIngredient> Ingredients { get; }
        public int? Grade { get; }

        [JsonIgnore]
        public bool ShoppingListHighlighted
        {
            get => shoppingListHighlighted;
            set
            {
                if (value == shoppingListHighlighted)
                    return;
                shoppingListHighlighted = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public BlueprintCategory Category
        {
            get
            {
                if (Engineers.FirstOrDefault() == "@Synthesis")
                {
                    return BlueprintCategory.Synthesis;
                }

                if (Type == "Unlock")
                {
                    return BlueprintCategory.Unlock;
                }

                if (Engineers.FirstOrDefault() == "@Technology")
                {
                    return BlueprintCategory.Technology;
                }

                if (Grade == null)
                {
                    return BlueprintCategory.Experimental;
                }

                return BlueprintCategory.Module;
            }
        }

        [JsonIgnore]
        public string TranslatedType => language.Translate(Type);

        [JsonIgnore]
        public string TranslatedName => language.Translate(BlueprintName);

        [JsonIgnore]
        public int ShoppingListCount
        {
            get => shoppingListCount;

            set
            {
                if (value == shoppingListCount)
                    return;
                shoppingListCount = value;
                OnPropertyChanged();
            }
        }

        public string SearchableContent { get; private set; }

        public Blueprint(ILanguage language, string type, string blueprintName, int? grade, IReadOnlyCollection<BlueprintIngredient> ingredients, IReadOnlyCollection<string> engineers)
        {
            this.language = language;

            Type = type;
            BlueprintName = blueprintName;
            Grade = grade;
            Engineers = engineers;
            Ingredients = ingredients;

            SetupSearchableContent();
            language.PropertyChanged += (o, e) => SetupSearchableContent();

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

        private void SetupSearchableContent()
        {
            var builder = new StringBuilder();
            builder.Append(language.Translate(ShortenedType) + "|");
            builder.Append(language.Translate(Type) + "|");
            builder.Append(language.Translate(BlueprintName) + "|");
            builder.Append(Prefix + "|");
            builder.Append(string.Join("|", Engineers) + "|");
            SearchableContent = builder.ToString().ToLowerInvariant();
        }

        [JsonIgnore]
        public bool Favorite
        {
            get => favorite;
            set
            {
                if (value == favorite) return;
                favorite = value;

                if (Category == BlueprintCategory.Synthesis)
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
            get => ignored;
            set
            {
                if (value == ignored)
                {
                    return;
                }

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
            return $"G{Grade} [{Type}] {BlueprintName}";
        }

        public string ShortString => $"{Prefix}{language.Translate(Type).Initials()} {language.Translate(BlueprintName).Initials()}";

        public string TranslatedString => $"{language.Translate(Type)} {language.Translate(BlueprintName)}";

        public string GradeString => Grade != null ? $"G{Grade}" : "🔹";

        private string Prefix
        {
            get
            {
                switch (Category)
                {
                    case BlueprintCategory.Synthesis:
                        return $"SYN ";
                    case BlueprintCategory.Experimental:
                        return $"EXP ";
                    case BlueprintCategory.Technology:
                        return $"TEC ";
                    case BlueprintCategory.Unlock:
                        return $"ULK ";
                    default:
                        return $"";
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object ToSerializable()
        {
            return new
            {
                BlueprintName,
                Type,
                Grade
            };
        }

        public bool HasSameIngredients(IReadOnlyCollection<BlueprintIngredient> ingredients)
        {
            return ingredients.Count == Ingredients.Count && ingredients.All(Ingredients.Contains);
        }
    }
}