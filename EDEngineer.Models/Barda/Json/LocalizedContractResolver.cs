using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EDEngineer.Models.Barda.Json
{
    public class LocalizedContractResolver : DefaultContractResolver
    {
        private readonly ILanguage translator;
        private readonly LanguageInfo lang;

        public LocalizedContractResolver(ILanguage translator, LanguageInfo lang)
        {
            this.translator = translator;
            this.lang = lang;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.PropertyName = translator.Translate(property.PropertyName, lang);
            return property;
        }
    }
}