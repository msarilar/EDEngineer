using System.Collections.Generic;
using System.ComponentModel;

namespace EDEngineer.Models
{
    public interface ILanguage : INotifyPropertyChanged
    {
        IDictionary<string, LanguageInfo> LanguageInfos { get; }
        LanguageInfo DefaultLanguage { get; }
        string Translate(string text);
        string Translate(string text, LanguageInfo langInfo);
        bool TryGetLangInfo(string lang, out LanguageInfo langInfo);
    }
}