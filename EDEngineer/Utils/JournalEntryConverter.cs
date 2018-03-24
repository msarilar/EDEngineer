using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Loadout;
using EDEngineer.Models.Operations;
using EDEngineer.Models.Utils;
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
        private readonly IEnumerable<Blueprint> blueprints;
        private static readonly HashSet<string> relevantJournalEvents = new HashSet<string>(Enum.GetNames(typeof(JournalEvent)));

        public JournalEntryConverter(ItemNameConverter converter, ISimpleDictionary<string, Entry> entries, Languages languages, IEnumerable<Blueprint> blueprints)
        {
            this.converter = converter;
            this.entries = entries;
            this.languages = languages;
            this.blueprints = blueprints;
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

        private JournalOperation ExtractOperation(JObject data, JournalEvent journalEvent)
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
                case JournalEvent.MaterialTrade:
                    return ExtractMaterialTrade(data);
                case JournalEvent.TechnologyBroker:
                    return ExtractTechnologyBroker(data);
                case JournalEvent.Location:
                case JournalEvent.FSDJump:
                    return ExtractSystemUpdated(data);
                case JournalEvent.Loadout:
                    return ExtractLoadout(data);
                default:
                    return null;
            }
        }

        private JournalOperation ExtractLoadout(JObject data)
        {
            var ship = (string) data["Ship"];
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

                Blueprint blueprint = null;
                string experimentalEffect = null;

                var engineering = module["Engineering"];
                var modifiers = new List<ModuleModifier>();
                if (engineering != null)
                {
                    experimentalEffect = (string)engineering["ExperimentalEffect"];
                    var name = (string) engineering["BlueprintName"];
                    foreach (var modifier in engineering["Modifiers"])
                    {
                        if (modifier["Value"]?.Type != JTokenType.Float)
                        {
                            continue;
                        }

                        var label = (string) modifier["Label"];
                        var value = (float) modifier["Value"];
                        var originalValue = (float) modifier["OriginalValue"];
                        var lessIsGood = (int) modifier["LessIsGood"];
                        modifiers.Add(new ModuleModifier(label, value, originalValue, lessIsGood == 1));
                    }
                }

                modules.Add(new ShipModule(type, slot, blueprint, experimentalEffect, modifiers));
            }


            return new ShipLoadoutOperation(new ShipLoadout(ship, shipName, shipIdent, shipValue, modulesValue, rebuy, modules));
        }

        private JournalOperation ExtractSystemUpdated(JObject data)
        {
            return new SystemUpdatedOperation((string) data["StarSystem"]);
        }

        private JournalOperation ExtractTechnologyBroker(JObject data)
        {
            var operation = new EngineerOperation(BlueprintCategory.Technology, null)
            {
                IngredientsConsumed = (data["Ingredients"] ?? data["Materials"]).Select(c =>
                    {
                        dynamic cc = c;
                        return Tuple.Create(converter.TryGet((string)cc.Name, out var ingredient), ingredient, (int)cc.Count);
                    })
                    .Where(c => c.Item1)
                    .Select(c => new BlueprintIngredient(entries[c.Item2], c.Item3)).ToList()
            };

            return operation.IngredientsConsumed.Any() ? operation : null;
        }

        private JournalOperation ExtractMaterialTrade(JObject data)
        {
            converter.TryGet((string)data["Received"]["Material"], out var ingredientAdded);
            converter.TryGet((string) data["Paid"]["Material"], out var ingredientRemoved);

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
                var materialName = converter.GetOrCreate((string)cc.Name);
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
            var dump = new DumpOperation
            {
                ResetFilter = new HashSet<Kind>
                {
                    Kind.Commodity
                },
                DumpOperations = new List<MaterialOperation>()
            };

            foreach (var jToken in data["Inventory"])
            {
                dynamic cc = jToken;
                var materialName = converter.GetOrCreate((string) cc.Name);
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

        private JournalOperation ExtractEngineerContribution(JObject data)
        {
            if (!converter.TryGet((string)data["Commodity"], out var name) &&
                !converter.TryGet((string)data["Material"], out name) &&
                !converter.TryGet((string)data["Encoded"], out name) &&
                !converter.TryGet((string)data["Raw"], out name) &&
                !converter.TryGet((string)data["Manufactured"], out name) &&
                !converter.TryGet((string)data["Data"], out name) &&
                !converter.TryGet((string)data["Commodity"], out name) &&
                !converter.TryGet((string)data["Name"], out name))
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
                    return null; // ignore commodity
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
            if (!converter.TryGet((string)data["Type"], out var marketSellName))
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

            if (!converter.TryGet(type, out var miningRefinedName))
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
            if (!converter.TryGet((string)data["Type"], out var marketBuyName))
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
            if (!converter.TryGet((string)data["Type"], out var ejectCargoName))
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
            if (!converter.TryGet((string)data["Type"], out var collectCargoName))
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
            if (!data.TryGetValue("MaterialsReward", out var rewardData))
            {
                return null;
            }

            var missionCompleted = new MissionCompletedOperation
            {
                CommodityRewards = rewardData
                    .Select(c => Tuple.Create(c,
                                converter.TryGet((string)c["Name"], out var rewardName),
                                rewardName))
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
            var materialDiscardedName = converter.GetOrCreate((string)data["Name"]);

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
            var synthesisOperation = new EngineerOperation(BlueprintCategory.Synthesis, null)
            {
                IngredientsConsumed = new List<BlueprintIngredient>()
            };

            foreach (var jToken in data["Materials"])
            {
                dynamic cc = jToken;
                var synthesisIngredientName = converter.GetOrCreate((string)cc.Name);
                int? count = cc.Value ?? cc.Count;

                synthesisOperation.IngredientsConsumed.Add(new BlueprintIngredient(entries[synthesisIngredientName],
                    count ?? 1));
            }

            return synthesisOperation.IngredientsConsumed.Any() ? synthesisOperation : null;
        }

        private JournalOperation ExtractMaterialCollected(JObject data)
        {
            var materialCollectedName = converter.GetOrCreate((string)data["Name"]);

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
            var blueprintName = data["Module"] ?? null;
            var operation = new EngineerOperation(BlueprintCategory.Module, (string) blueprintName)
            {
                IngredientsConsumed = data["Ingredients"].Select(c =>
                {
                    dynamic cc = c;
                    return Tuple.Create(converter.TryGet((string)cc.Name, out var rewardName), rewardName, (int)(cc.Value ?? cc.Count));
                }).Where(c => c.Item1).Select(c => new BlueprintIngredient(entries[c.Item2], c.Item3)).ToList()
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