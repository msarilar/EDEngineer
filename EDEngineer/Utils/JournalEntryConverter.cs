using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Loadout;
using EDEngineer.Models.Operations;
using EDEngineer.Models.Utils.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime.Text;

namespace EDEngineer.Utils
{
    public class JournalEntryConverter : JsonConverter
    {
        private readonly ItemNameConverter converter;
        private readonly ISimpleDictionary<string, Entry> entries;
        private readonly Languages languages;
        private static readonly HashSet<string> relevantJournalEvents = new HashSet<string>(Enum.GetNames(typeof(JournalEvent)));

        public JournalEntryConverter(ItemNameConverter converter, ISimpleDictionary<string, Entry> entries, Languages languages, IEnumerable<Blueprint> blueprints)
        {
            this.converter = converter;
            this.entries = entries;
            this.languages = languages;
        }

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (JournalEntry);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            reader.DateParseHandling = DateParseHandling.None;

            JObject data;

            try
            {
                data = JObject.Load(reader);
            }
            catch(JsonReaderException)
            {
                // malformed json outputted by the game, nothing we can do here
                return new JournalEntry();
            }

            var entry = new JournalEntry
            {
                Timestamp = InstantPattern.General.Parse((string)data["timestamp"]).Value,
                OriginalJson = data.ToString()
            };

            JournalEvent? journalEvent = null;

            try
            {
                var eventString =(string) data["event"];

                if (relevantJournalEvents.Contains(eventString))
                {
                    journalEvent = data["event"]?.ToObject<JournalEvent>(serializer);
                }
            }
            catch (Exception)
            {
                return entry;
            }

            if (!journalEvent.HasValue)
            {
                return entry;
            }

            try
            {
                entry.JournalOperation = ExtractOperation(data, journalEvent.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    languages.Translate("Something went wrong in parsing your logs, open an issue on GitHub with this information : ") + 
                    Environment.NewLine +
                    $"LogEntry = {data}{Environment.NewLine}" +
                    $"Error:{e}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (entry.JournalOperation != null)
            {
                entry.JournalOperation.JournalEvent = journalEvent.Value;
            }

            return entry;
        }

        public JournalOperation ExtractOperation(JObject data, JournalEvent journalEvent)
        {
            switch (journalEvent)
            {
                case JournalEvent.ManualUserChange:
                    return ExctractManualOperation(data);
                case JournalEvent.MiningRefined:
                    return ExtractMiningRefined(data);
                case JournalEvent.EngineerCraft:
                    return ExtractEngineerOperation(data);
                case JournalEvent.MarketSell:
                    return ExtractMarketSell(data);
                case JournalEvent.MarketBuy:
                    return ExtractMarketBuy(data);
                case JournalEvent.MaterialDiscarded:
                    return ExtractMaterialDiscarded(data);
                case JournalEvent.MaterialCollected:
                    return ExtractMaterialCollected(data);
                case JournalEvent.MissionCompleted:
                    return ExtractMissionCompleted(data);
                case JournalEvent.CollectCargo:
                    return ExtractCollectCargo(data);
                case JournalEvent.EjectCargo:
                    return ExtractEjectCargo(data);
                case JournalEvent.Synthesis:
                    return ExtractSynthesis(data);
                case JournalEvent.EngineerContribution:
                    return ExtractEngineerContribution(data);
                case JournalEvent.ScientificResearch:
                    return ExtractMaterialDiscarded(data);
                case JournalEvent.Materials:
                    return ExtractMaterialsDump(data);
                case JournalEvent.Cargo:
                    return ExtractCargoDump(data);
                case JournalEvent.MaterialTrade:
                    return ExtractMaterialTrade(data);
                case JournalEvent.TechnologyBroker:
                    return ExtractTechnologyBroker(data);
                case JournalEvent.Location:
                case JournalEvent.FSDJump:
                    return ExtractSystemUpdated(data);
                case JournalEvent.Loadout:
                    return ExtractLoadout(data);
                case JournalEvent.Died:
                    return ExtractDied(data);
                default:
                    return null;
            }
        }

        private JournalOperation ExtractDied(JObject _)
        {
            return new DumpOperation
            {
                ResetFilter = new HashSet<Kind> { Kind.Commodity },
                DumpOperations = new List<MaterialOperation>()
            };
        }

        private JournalOperation ExtractLoadout(JObject data)
        {
            var ship = (string) data["Ship"];
            if (ship.ToLowerInvariant().Contains("fighter"))
            {
                return null;
            }

            var shipName = (string)data["ShipName"];
            var shipIdent = (string)data["ShipIdent"];
            var shipValue = (int?)data["HullValue"];
            var modulesValue = (int?)data["ModulesValue"];
            var rebuy = (int?)data["Rebuy"];

            var modules = new List<ShipModule>();
            foreach (var module in data["Modules"])
            {
                var type = (string)module["Item"];
                var slot = (string)module["Slot"];

                string experimentalEffect = null;
                int? grade = null;
                string blueprintName = null;
                string engineer = null;

                var engineering = module["Engineering"];
                var modifiers = new List<ModuleModifier>();
                if (engineering != null && engineering.Count() > 0)
                {
                    engineer = (string) engineering["Engineer"];
                    experimentalEffect = (string)engineering["ExperimentalEffect_Localised"];
                    grade = (int) engineering["Level"];
                    blueprintName = (string) engineering["BlueprintName"];
                    var modifierSource = engineering["Modifiers"];
                    modifiers.AddRange(ExtractModifiers(modifierSource));
                }

                modules.Add(new ShipModule(type, slot, blueprintName, grade, engineer, experimentalEffect, modifiers));
            }


            return new ShipLoadoutOperation(new ShipLoadout(ship, shipName, shipIdent, shipValue, modulesValue, rebuy, modules.OrderBy(m => m.Category).ToList()));
        }

        private static IEnumerable<ModuleModifier> ExtractModifiers(JToken modifierSource)
        {
            if (modifierSource == null)
            {
                return Enumerable.Empty<ModuleModifier>();
            }

            return from modifier
                       in modifierSource
                   where modifier["Value"]?.Type == JTokenType.Float
                   let label = (string) modifier["Label"]
                   let value = (float) modifier["Value"]
                   let originalValue = (float?) modifier["OriginalValue"]
                   let lessIsGood = (int) modifier["LessIsGood"]
                   select new ModuleModifier(label, value, originalValue, lessIsGood == 1);
        }

        private JournalOperation ExtractSystemUpdated(JObject data)
        {
            return new SystemUpdatedOperation((string) data["StarSystem"]);
        }

        private JournalOperation ExtractTechnologyBroker(JObject data)
        {
            var operation = new EngineerOperation(BlueprintCategory.Technology, null, null, null, null, null, null)
            {
                IngredientsConsumed = (data["Ingredients"]?.Select(c =>
                    {
                        dynamic cc = c;
                        return Tuple.Create(converter.TryGet(Kind.Data | Kind.Material | Kind.Commodity, (string)cc.Name, out var ingredient), ingredient, (int)cc.Count);
                    }) ?? Enumerable.Empty<Tuple<bool, string, int>>())
                    .Union(data["Materials"]?.Select(c =>
                    {
                        dynamic cc = c;
                        var filter = cc.Category == "Encoded" ? Kind.Data : Kind.Material;
                        return Tuple.Create(converter.TryGet(filter, (string)cc.Name, out var ingredient), ingredient, (int)cc.Count);
                    }) ?? Enumerable.Empty<Tuple<bool, string, int>>())
                    .Union(data["Commodities"]?.Select(c =>
                    {
                        dynamic cc = c;
                        return Tuple.Create(converter.TryGet(Kind.Commodity, (string)cc.Name, out var ingredient), ingredient, (int)cc.Count);
                    }) ?? Enumerable.Empty<Tuple<bool, string, int>>())
                    .Where(c => c.Item1)
                    .Select(c => new BlueprintIngredient(entries[c.Item2], c.Item3)).ToList()
            };

            return operation.IngredientsConsumed.Any() ? operation : null;
        }

        private JournalOperation ExtractMaterialTrade(JObject data)
        {
            converter.TryGet(Kind.Data | Kind.Material, (string)data["Received"]["Material"], out var ingredientAdded);
            converter.TryGet(Kind.Data | Kind.Material, (string) data["Paid"]["Material"], out var ingredientRemoved);

            var addedQuantity = (int) data["Received"]["Quantity"];
            var removedQuantity = (int)data["Paid"]["Quantity"];

            return new MaterialTradeOperation
            {
                IngredientAdded = ingredientAdded,
                IngredientRemoved = ingredientRemoved,
                AddedQuantity = addedQuantity,
                RemovedQuantity = removedQuantity
            };
        }

        private JournalOperation ExtractMaterialsDump(JObject data)
        {
            var dump = new DumpOperation
            {
                ResetFilter = new HashSet<Kind>
                {
                    Kind.Data,
                    Kind.Material
                },
                DumpOperations = new List<MaterialOperation>()
            };

            foreach (var jToken in data["Raw"].Union(data["Manufactured"]).Union(data["Encoded"]))
            {
                dynamic cc = jToken;
                var materialName = converter.GetOrCreate(Kind.Data | Kind.Material, (string)cc.Name);
                int? count = cc.Value ?? cc.Count;

                var operation = new MaterialOperation
                {
                    MaterialName = materialName,
                    Size = count ?? 1
                };

                dump.DumpOperations.Add(operation);
            }

            return dump;
        }

        private JournalOperation ExtractCargoDump(JObject data)
        {
            // ED version 3.3 (December 11th 2018) made some breaking changes:
            //  - Cargo event was added after buying/selling/scooping/ejecting commodities/limpets
            //  - But unfortunately this Cargo event is different to normal Cargo event and does not contain the Inventory key (so it needs to be ignored)
            //  - Note that when cmdr is loaded/game is started, Cargo event DOES contain the Inventory field
            if (data["Inventory"] == null)
            {
                return null;
            }

            var dump = new DumpOperation
            {
                ResetFilter = new HashSet<Kind>
                {
                    Kind.Commodity
                },
                DumpOperations = new List<MaterialOperation>()
            };

            var inventoryData = data["Inventory"];

            if (inventoryData != null)
                foreach (var jToken in inventoryData)
                {
                    dynamic cc = jToken;
                    if (!converter.TryGet(Kind.Commodity, (string) cc.Name, out var commodityName))
                    {
                        continue;
                    }

                    int? count = cc.Value ?? cc.Count;

                    var operation = new MaterialOperation
                    {
                        MaterialName = commodityName,
                        Size = count ?? 1
                    };

                    dump.DumpOperations.Add(operation);
                }

            return dump;
        }

        private JournalOperation ExtractEngineerContribution(JObject data)
        {
            if (!converter.TryGet(Kind.Commodity, (string)data["Commodity"], out var name) &&
                !converter.TryGet(Kind.Material, (string)data["Material"], out name) &&
                !converter.TryGet(Kind.Data, (string)data["Encoded"], out name) &&
                !converter.TryGet(Kind.Material, (string)data["Raw"], out name) &&
                !converter.TryGet(Kind.Material, (string)data["Manufactured"], out name) &&
                !converter.TryGet(Kind.Data, (string)data["Data"], out name) &&
                !converter.TryGet(Kind.Data | Kind.Material | Kind.Commodity, (string)data["Name"], out name))
            {
                return null;
            }

            var type = ((string) data["Type"]).ToLowerInvariant();
            switch (type)
            {
                case "encoded":
                case "data":
                    return new DataOperation
                    {
                        DataName = name,
                        Size = -1 * data["Quantity"]?.ToObject<int>() ?? 1
                    };
                case "commodity":
                    return new CargoOperation
                    {
                        CommodityName = name,
                        Size = -1 * data["Quantity"]?.ToObject<int>() ?? 1
                    };
                default: // materials
                    return new MaterialOperation
                    {
                        MaterialName = name,
                        Size = -1 * data["Quantity"]?.ToObject<int>() ?? 1
                    };
            }
        }

        private JournalOperation ExtractMarketSell(JObject data)
        {
            if (!converter.TryGet(Kind.Commodity, (string)data["Type"], out var marketSellName))
            {
                return null;
            }

            return new CargoOperation
            {
                CommodityName = marketSellName,
                Size = -1*data["Count"]?.ToObject<int>() ?? -1
            };
        }

        private JournalOperation ExtractMiningRefined(JObject data)
        {
            var type = (string)data["Type"];

            type = type.Replace("$", "").Replace("_name;", ""); // "Type":"$samarium_name;" 

            if (!converter.TryGet(Kind.Material | Kind.Commodity, type, out var miningRefinedName))
            {
                return null;
            }

            return new CargoOperation
            {
                CommodityName = miningRefinedName,
                Size = 1
            };
        }

        private JournalOperation ExtractMarketBuy(JObject data)
        {
            if (!converter.TryGet(Kind.Commodity, (string)data["Type"], out var marketBuyName))
            {
                return null;
            }

            return new CargoOperation
            {
                CommodityName = marketBuyName,
                Size = data["Count"]?.ToObject<int>() ?? 1
            };
        }

        private JournalOperation ExtractEjectCargo(JObject data)
        {
            if (!converter.TryGet(Kind.Commodity, (string)data["Type"], out var ejectCargoName))
            {
                return null;
            }

            return new CargoOperation
            {
                CommodityName = ejectCargoName,
                Size = -1 * data["Count"]?.ToObject<int>() ?? -1
            };
        }

        private JournalOperation ExtractCollectCargo(JObject data)
        {
            if (!converter.TryGet(Kind.Commodity, (string)data["Type"], out var collectCargoName))
            {
                return null;
            }

            return new CargoOperation
            {
                CommodityName = collectCargoName,
                Size = 1
            };
        }

        private JournalOperation ExtractMissionCompleted(JObject data)
        {
            var missionCompleted = new MissionCompletedOperation
            {
                CommodityRewards = (data["MaterialsReward"]
                    ?.Select(c => Tuple.Create(c,
                                converter.TryGet(Kind.Data | Kind.Material, (string)c["Name"], out var rewardName),
                                rewardName)) ?? Enumerable.Empty<Tuple<JToken, bool, string>>())
                    .Union(data["CommodityReward"]?.Select(c => Tuple.Create(c,
                                    converter.TryGet(Kind.Commodity, (string)c["Name"], out var rewardName),
                                    rewardName)) ?? Enumerable.Empty<Tuple<JToken, bool, string>>())
                    .Where(c => c.Item2)
                    .Select(c =>
                    {
                        var r = new CargoOperation
                        {
                            CommodityName = c.Item3,
                            Size = c.Item1["Count"]?.ToObject<int>() ?? 1,
                            JournalEvent = JournalEvent.MissionCompleted
                        };
                        return r;
                    }).ToList()
            };

            return missionCompleted.CommodityRewards.Any() ? missionCompleted : null;
        }

        private JournalOperation ExtractMaterialDiscarded(JObject data)
        {
            var materialDiscardedName = converter.GetOrCreate(Kind.Data | Kind.Material, (string)data["Name"]);

            if (((string) data["Category"]).ToLowerInvariant() == "encoded")
            {
                return new DataOperation
                {
                    DataName = materialDiscardedName,
                    Size = -1*data["Count"]?.ToObject<int>() ?? -1
                };
            }
            else // Manufactured & Raw
            {
                return new MaterialOperation
                {
                    MaterialName = materialDiscardedName,
                    Size = -1*data["Count"]?.ToObject<int>() ?? -1
                };
            }
        }

        private JournalOperation ExtractSynthesis(JObject data)
        {
            var synthesisOperation = new EngineerOperation(BlueprintCategory.Synthesis, null, null, null, null, null, null)
            {
                IngredientsConsumed = new List<BlueprintIngredient>()
            };

            foreach (var jToken in data["Materials"])
            {
                dynamic cc = jToken;
                var synthesisIngredientName = converter.GetOrCreate(Kind.Material, (string)cc.Name);
                int? count = cc.Value ?? cc.Count;

                synthesisOperation.IngredientsConsumed.Add(new BlueprintIngredient(entries[synthesisIngredientName],
                    count ?? 1));
            }

            return synthesisOperation.IngredientsConsumed.Any() ? synthesisOperation : null;
        }

        private JournalOperation ExtractMaterialCollected(JObject data)
        {
            var materialCollectedName = converter.GetOrCreate(Kind.Data | Kind.Material, (string)data["Name"]);

            if (((string) data["Category"]).ToLowerInvariant() == "encoded")
            {
                return new DataOperation
                {
                    DataName = materialCollectedName, Size = data["Count"]?.ToObject<int>() ?? 1
                };
            }
            else // Manufactured & Raw
            {
                return new MaterialOperation
                {
                    MaterialName = materialCollectedName, Size = data["Count"]?.ToObject<int>() ?? 1
                };
            }
        }

        private JournalOperation ExtractEngineerOperation(JObject data)
        {
            var operation = new EngineerOperation(BlueprintCategory.Module, (string) data["BlueprintName"],
                (string) data["Module"], (string) data["Slot"], (string)data["Engineer"], (int?)data["Level"], (string)data["ApplyExperimentalEffect"])
            {
                IngredientsConsumed = data["Ingredients"].Select(c =>
                {
                    dynamic cc = c;
                    return Tuple.Create(converter.TryGet(Kind.Data | Kind.Material | Kind.Commodity, (string)cc.Name, out var rewardName), rewardName, (int)(cc.Value ?? cc.Count));
                }).Where(c => c.Item1).Select(c => new BlueprintIngredient(entries[c.Item2], c.Item3)).ToList(),
                Modifiers = ExtractModifiers(data["Modifiers"]).ToList()
            };

            return operation.IngredientsConsumed.Any() ? operation : null;
        }

        private static JournalOperation ExctractManualOperation(JObject data)
        {
            return new ManualChangeOperation
            {
                Name = (string) data["Name"], Count = (int) data["Count"]
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var entry = (JournalEntry) value;
            var operation = (ManualChangeOperation) entry.JournalOperation;
            writer.WriteStartObject();
            writer.WritePropertyName("timestamp");
            writer.WriteValue(entry.Timestamp.ToString(InstantPattern.General.PatternText, CultureInfo.InvariantCulture));
            writer.WritePropertyName("event");
            writer.WriteValue(operation.JournalEvent.ToString());
            writer.WritePropertyName("Name");
            writer.WriteValue(operation.Name);
            writer.WritePropertyName("Count");
            writer.WriteValue(operation.Count);
            writer.WriteEndObject();
        }
    }
}