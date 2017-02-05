using System.Collections.Generic;
using System.ComponentModel;

namespace EDEngineer.Models
{
    public interface ILanguage
    {
        IDictionary<string, LanguageInfo> LanguageInfos { get; }
        LanguageInfo DefaultLanguage { get; }
        string Translate(string text);
        event PropertyChangedEventHandler PropertyChanged;
        string Translate(string text, LanguageInfo langInfo);
        bool TryGetLangInfo(string lang, out LanguageInfo langInfo);
    }
}