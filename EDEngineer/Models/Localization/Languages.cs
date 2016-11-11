using System.Collections.Generic;

namespace EDEngineer.Models.Localization
{
    public class Languages
    {
        public List<LanguageInfo> LanguageInfos { get; set; }
        public Dictionary<string, Translation> Translations { get; set; }
    }
}