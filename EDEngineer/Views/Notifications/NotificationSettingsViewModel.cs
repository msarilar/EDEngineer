using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Utils;
using EDEngineer.Utils.System;
using Newtonsoft.Json;

namespace EDEngineer.Views.Notifications
{
    public class NotificationSettingsViewModel : INotifyPropertyChanged, IDisposable
    {
        private NotificationKind notificationBlueprint;
        private readonly CommanderNotifications testCommanderNotifications;
        private readonly State testState;
        private readonly Random random;
        private Tuple<string, string> selectedVoice;
        public Languages Languages { get; }

        public NotificationKind NotificationKindBlueprintReady
        {
            get => notificationBlueprint;
            set
            {
                notificationBlueprint = value;
                SettingsManager.NotificationKindBlueprintReady = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Tuple<string, string>> Voices => testCommanderNotifications.AvailableLanguages();

        public Tuple<string, string> SelectedVoice
        {
            get => selectedVoice;
            set
            {
                selectedVoice = value;
                OnPropertyChanged();
                SettingsManager.NotificationVoice = value.Item2;
            }
        }

        public NotificationSettingsViewModel(Languages languages)
        {
            random = new Random();
            Languages = languages;

            testState = new State(JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson()), languages, "Count");
            testState.Blueprints = new List<Blueprint>(JsonConvert.DeserializeObject<List<Blueprint>>(IOUtils.GetBlueprintsJson(), new BlueprintConverter(testState.Cargo)));

            testCommanderNotifications = new CommanderNotifications(testState);
            testCommanderNotifications.SubscribeNotifications();

            NotificationKindBlueprintReady = SettingsManager.NotificationKindBlueprintReady;

            SelectedVoice = Voices.FirstOrDefault(v => v.Item2 == SettingsManager.NotificationVoice) ?? Voices.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetState()
        {
            foreach (var entryData in testState.Cargo.Values)
            {
                entryData.Count = 0;
            }

            foreach (var blueprint in testState.Blueprints)
            {
                blueprint.Favorite = false;
            }
        }

        public void TriggerFavoriteReady()
        {
            ResetState();
            var blueprint = testState.Blueprints.Skip(random.Next(0, testState.Blueprints.Count)).First();
            blueprint.Favorite = true;

            foreach (var ingredient in blueprint.Ingredients)
            {
                testState.IncrementCargo(ingredient.Entry.Data.Name, ingredient.Size);
            }
        }

        public void Dispose()
        {
            testCommanderNotifications.UnsubscribeNotifications();
        }
    }
}
