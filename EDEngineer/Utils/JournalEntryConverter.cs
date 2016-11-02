using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDEngineer.Models;
using EDEngineer.Models.Operations;
using EDEngineer.Utils.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime.Text;

namespace EDEngineer.Utils
{
    public class JournalEntryConverter : JsonConverter
    {
        private readonly ISimpleDictionary<string, Entry> entries;

        public JournalEntryConverter(ISimpleDictionary<string, Entry> entries)
        {
            this.entries = entries;
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
            var data = JObject.Load(reader);

            var entry = new JournalEntry
            {
                TimeStamp = InstantPattern.GeneralPattern.Parse((string) data["timestamp"]).Value,
                OriginalJson = data.ToString()
            };

            JournalEvent journalEvent;

            try
            {
                journalEvent = data["event"].ToObject<JournalEvent>(serializer);
            }
            catch (JsonSerializationException)
            {
                return entry;
            }

            switch (journalEvent)
            {
                case JournalEvent.ManualUserChange:
                    entry.JournalOperation = new ManualChangeOperation
                    {
                        Name = (string) data["Name"],
                        Count = (int) data["Count"]
                    };
                    break;
                case JournalEvent.MiningRefined:
                    string miningRefinedName;
                    if (!ItemNameConverter.TryGet((string) data["Type"], out miningRefinedName))
                    {
                        break;
                    }

                    entry.JournalOperation = new CargoOperation
                    {
                        CommodityName = miningRefinedName,
                        Size = 1
                    };
                    break;
                case JournalEvent.EngineerCraft:
                    var engineerCraft = new EngineerOperation
                    {
                        IngredientsConsumed = data["Ingredients"]
                            .Select(c =>
                            {
                                dynamic cc = c;
                                string rewardName;
                                return Tuple.Create(ItemNameConverter.TryGet((string) cc.Name, out rewardName),
                                    rewardName,
                                    (int) cc.Value);
                            })
                            .Where(c => c.Item1)
                            .Select(c => new BlueprintIngredient(entries[c.Item2], c.Item3)).ToList()
                    };

                    if (engineerCraft.IngredientsConsumed.Any())
                    {
                        entry.JournalOperation = engineerCraft;
                    }
                    break;
                case JournalEvent.MarketSell:
                    string marketSellName;
                    if (!ItemNameConverter.TryGet((string) data["Type"], out marketSellName))
                    {
                        break;
                    }

                    entry.JournalOperation = new CargoOperation
                    {
                        CommodityName = marketSellName,
                        Size = -1*data["Count"].ToObject<int>()
                    };
                    break;
                case JournalEvent.MarketBuy:
                    string marketBuyName;
                    if (!ItemNameConverter.TryGet((string) data["Type"], out marketBuyName))
                    {
                        break;
                    }

                    entry.JournalOperation = new CargoOperation
                    {
                        CommodityName = marketBuyName,
                        Size = data["Count"].ToObject<int>()
                    };
                    break;
                case JournalEvent.MaterialDiscarded:
                    string materialDiscardedName;
                    if (!ItemNameConverter.TryGet((string) data["Name"], out materialDiscardedName))
                    {
                        break;
                    }

                    if (((string) data["Category"]).ToLowerInvariant() == "encoded")
                    {
                        entry.JournalOperation = new DataOperation
                        {
                            DataName = materialDiscardedName,
                            Size = -1*data["Count"].ToObject<int>()
                        };
                    }
                    else // Manufactured & Raw
                    {
                        entry.JournalOperation = new MaterialOperation
                        {
                            MaterialName = materialDiscardedName,
                            Size = -1*data["Count"].ToObject<int>()
                        };
                    }

                    break;
                case JournalEvent.MaterialCollected:
                    string materialCollectedName;
                    if (!ItemNameConverter.TryGet((string) data["Name"], out materialCollectedName))
                    {
                        break;
                    }

                    if (((string) data["Category"]).ToLowerInvariant() == "encoded")
                    {
                        entry.JournalOperation = new DataOperation
                        {
                            DataName = materialCollectedName,
                            Size = data["Count"].ToObject<int>()
                        };
                    }
                    else // Manufactured & Raw
                    {
                        entry.JournalOperation = new MaterialOperation
                        {
                            MaterialName = materialCollectedName,
                            Size = data["Count"].ToObject<int>()
                        };
                    }

                    break;
                case JournalEvent.MissionCompleted:
                    JToken rewardData;

                    // TODO: missions can sometimes reward data/material and this is not currently handled

                    if (!data.TryGetValue("CommodityReward", out rewardData))
                    {
                        break;
                    }

                    var missionCompleted = new MissionCompletedOperation
                    {
                        CommodityRewards = rewardData
                            .Select(c =>
                            {
                                string rewardName;
                                return Tuple.Create(c,
                                    ItemNameConverter.TryGet((string) c["Name"], out rewardName),
                                    rewardName);
                            })
                            .Where(c => c.Item2)
                            .Select(c =>
                            {
                                var r = new CargoOperation
                                {
                                    CommodityName = c.Item3,
                                    Size = c.Item1["Count"].ToObject<int>(),
                                    JournalEvent = JournalEvent.MissionCompleted
                                };
                                return r;
                            }).ToList()
                    };
                    if (missionCompleted.CommodityRewards.Any())
                    {
                        entry.JournalOperation = missionCompleted;
                    }

                    break;
                case JournalEvent.CollectCargo:
                    string collectCargoName;
                    if (!ItemNameConverter.TryGet((string) data["Type"], out collectCargoName))
                    {
                        break;
                    }

                    entry.JournalOperation = new CargoOperation
                    {
                        CommodityName = collectCargoName,
                        Size = 1
                    };
                    break;
                case JournalEvent.EjectCargo:
                    string ejectCargoName;
                    if (!ItemNameConverter.TryGet((string) data["Type"], out ejectCargoName))
                    {
                        break;
                    }

                    entry.JournalOperation = new CargoOperation
                    {
                        CommodityName = ejectCargoName,
                        Size = -1
                    };
                    break;
            }

            if (entry.JournalOperation != null)
            {
                entry.JournalOperation.JournalEvent = journalEvent;
            }

            return entry;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var entry = (JournalEntry) value;
            var operation = (ManualChangeOperation) entry.JournalOperation;
            writer.WriteStartObject();
            writer.WritePropertyName("timestamp");
            writer.WriteValue(entry.TimeStamp.ToString(InstantPattern.GeneralPattern.PatternText,
                CultureInfo.InvariantCulture));
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