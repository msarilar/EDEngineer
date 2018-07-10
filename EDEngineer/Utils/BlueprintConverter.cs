using System;
using System.Linq;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Utils.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDEngineer.Utils
{
    public class BlueprintConverter : JsonConverter
    {
        private readonly ISimpleDictionary<string, Entry> entries;

        public BlueprintConverter(ISimpleDictionary<string, Entry> entries)
        {
            this.entries = entries;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = JObject.Load(reader);

            var blueprintName = (string) data["Name"];
            var blueprintType = (string)data["Type"];
            var blueprintGrade = (int?)data["Grade"];

            var engineers = data["Engineers"].Select(engineer => (string) engineer).ToList();

            var ingredients =
                from ingredient in data["Ingredients"]
                let name = (string) ingredient["Name"]
                let size = (int) ingredient["Size"]
                where entries[name].Data.Kind != Kind.Commodity || engineers.First() == "@Technology" || blueprintType == "Unlock"
                select new BlueprintIngredient(entries[name], size);

            var effects = data["Effects"] == null ? Enumerable.Empty<BlueprintEffect>() :
                from json in data["Effects"]
                let effect = (string)json["Effect"]
                let property = (string)json["Property"]
                let isGood = (bool)json["IsGood"]
                select new BlueprintEffect(property, effect, isGood);

            var token = (string) data["CoriolisGuid"];
            var coriolisGuid = token == null ? (Guid?) null : Guid.Parse(token);

            return new Blueprint(Languages.Instance, blueprintType, blueprintName, blueprintGrade, ingredients.ToList(), engineers, effects.ToList(), coriolisGuid);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Blueprint);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}