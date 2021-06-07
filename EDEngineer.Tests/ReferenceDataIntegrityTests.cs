using System.Collections.Generic;
using System.Linq;
using EDEngineer.Localization;
using EDEngineer.Models.Utils;
using EDEngineer.Models.Utils.Json;
using EDEngineer.Tests.StrippedDownModels;
using EDEngineer.Utils.System;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;

namespace EDEngineer.Tests
{
    [TestFixture]
    public class ReferenceDataIntegrityTests
    {
        [Test]
        public void Can_load_blueprints()
        {
            Check.ThatCode(() => JsonConvert.DeserializeObject<List<Blueprint>>(Helpers.GetBlueprintsJson(), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            })).DoesNotThrow();
        }

        [Test]
        public void Blueprints_are_properly_indented()
        {
            var json = Helpers.GetBlueprintsJson().TrimEnd();
            var blueprints = JsonConvert.DeserializeObject<List<Blueprint>>(json);
            var serialized = JsonConvert.SerializeObject(blueprints, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            Check.That(serialized).IsEqualTo(json);
        }

        [Test]
        public void No_single_blueprint_type()
        {
            // known single blueprint types:
            var exclusionList = new HashSet<string>
            {
                "Limpets"
            };

            var blueprints = JsonConvert.DeserializeObject<List<Blueprint>>(Helpers.GetBlueprintsJson());
            var singleTypes = blueprints.GroupBy(b => b.Type).Where(kv => !exclusionList.Contains(kv.Key) && kv.Count() == 1);
            Check.That(singleTypes).IsEmpty();
        }

        [Test]
        public void No_engineer_mistyped()
        {
            var existingEngineers = new HashSet<string>
            {
                "@Synthesis",
                "@Technology",
                "@Merchant",
                "Bill Turner",
                "Broo Tarquin",
                "Colonel Bris Dekker",
                "Didi Vatermann",
                "Elvira Martuuk",
                "Felicity Farseer",
                "Hera Tani",
                "Juri Ishmaak",
                "Lei Cheung",
                "Liz Ryder",
                "Lori Jameson",
                "Marco Qwent",
                "Professor Palin",
                "Ram Tah",
                "Selene Jean",
                "The Dweller",
                "The Sarge",
                "Tiana Fortune",
                "Tod McQuinn",
                "Zacariah Nemo",
                "Petra Olmanova",
                "Marsha Hicks",
                "Etienne Dorn",
                "Mel Brandon",
                "Chloe Sedesi",
                // odyssey!
                "Domino Green",
                "Oden Geiger",
                "Wellington Beck",
                "Kit Fowler",
                "Jude Navarro",
                "Uma Laszlo",
                "Hero Ferrari",
                "Terra Velasquez"
            };

            var blueprints = JsonConvert.DeserializeObject<List<Blueprint>>(Helpers.GetBlueprintsJson());
            var engineers = blueprints.SelectMany(b => b.Engineers).ToHashSet();

            Check.That(engineers).ContainsOnlyElementsThatMatch(s => existingEngineers.Contains(s));
        }

        [Test]
        public void Blueprints_ingredients_exist()
        {
            var blueprints = JsonConvert.DeserializeObject<List<Blueprint>>(Helpers.GetBlueprintsJson());
            var entries = JsonConvert.DeserializeObject<List<EntryData>>(Helpers.GetEntryDatasJson());

            var ingredients = blueprints.SelectMany(b => b.Ingredients).Select(i => i.Name).ToHashSet();
            var materials = entries.Select(e => e.Name).ToHashSet();

            var unknownIngredients = ingredients.Where(i => !materials.Contains(i)).ToList();
            Check.That(unknownIngredients).IsEmpty();

            Check.That(ingredients).ContainsOnlyElementsThatMatch(i => materials.Contains(i));
        }

        [Test]
        public void Can_load_ingredients()
        {
            Check.ThatCode(() => JsonConvert.DeserializeObject<List<EntryData>>(Helpers.GetEntryDatasJson(), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Ignore
            })).DoesNotThrow();
        }

        [Test]
        public void Ingredients_are_properly_indented()
        {
            var json = Helpers.GetEntryDatasJson();
            var entries = JsonConvert.DeserializeObject<List<EntryData>>(json);
            var serialized = JsonConvert.SerializeObject(entries, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            Check.That(serialized).IsEqualTo(json);
        }

        [Test]
        public void Can_load_localization()
        {
            Check.ThatCode(() => JsonConvert.DeserializeObject<Languages>(Helpers.GetLocalizationJson(), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include
            })).DoesNotThrow();
        }

        [Test]
        public void No_missing_string_format_indicators()
        {
            var localization = JsonConvert.DeserializeObject<Languages>(Helpers.GetLocalizationJson());
            Check.That(localization.Translations
                                   .Where(kv => kv.Key.Contains("{0}") &&
                                                kv.Value.Any(t => t.Value != null && !t.Value.Contains("{0}"))))
                 .IsEmpty();
        }
    }
}
