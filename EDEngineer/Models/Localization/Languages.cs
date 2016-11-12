using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EDEngineer.Properties;
using EDEngineer.Utils.System;
using Newtonsoft.Json;

namespace EDEngineer.Models.Localization
{
    public class Languages : INotifyPropertyChanged
    {
        private const string DEFAULT_LANG = "en";

        private LanguageInfo currentLanguage;
        public Dictionary<string, LanguageInfo> LanguageInfos { get; set; }
        public Dictionary<string, Translation> Translations { get; set; }

        public LanguageInfo CurrentLanguage
        {
            get
            {
                return currentLanguage;
            }
            set
            {
                currentLanguage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Languages InitLanguages()
        {
            var json = IOUtils.GetLocalizationJson();
            var languages = JsonConvert.DeserializeObject<Languages>(json);

            var total = CheckForExistingTranslation(languages);
            ComputeProgress(languages, total);

            // CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            // CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            return languages;
        }

        private static double CheckForExistingTranslation(Languages languages)
        {
            var total = 0d;
            foreach (var text in languages.Translations.Keys)
            {
                total++;

                var translation = languages.Translations[text];
                foreach (var lang in translation.Keys)
                {
                    var translatedText = translation[lang];
                    if (translatedText != null)
                    {
                        languages.LanguageInfos[lang].Progress += 1;
                    }
                }
            }
            return total;
        }

        private static void ComputeProgress(Languages languages, double total)
        {
            foreach (var lang in languages.LanguageInfos.Keys)
            {
                if (lang == DEFAULT_LANG)
                {
                    languages.LanguageInfos[lang].Progress = 1;
                }
                else
                {
                    languages.LanguageInfos[lang].Progress /= total;
                }

                languages.LanguageInfos[lang].Progress *= 100;
            }
        }
    }
}