using System;
using Newtonsoft.Json;

namespace EDEngineer.Models.Barda.Json
{
    public class LocalizedJsonConverter : JsonConverter
    {
        private readonly ILanguage translator;
        private readonly LanguageInfo lang;

        public LocalizedJsonConverter(ILanguage translator, LanguageInfo lang)
        {
            this.translator = translator;
            this.lang = lang;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(translator.Translate((string) value, lang));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}