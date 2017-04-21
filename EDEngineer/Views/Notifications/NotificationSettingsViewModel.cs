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
        private NotificationKind notificationKind;
        private bool thresholdReachedEnabled;
        private bool cargoAlmostFullEnabled;
        private bool FavoriteBlueprintEnabled;
        private readonly CommanderNotifications testCommanderNotifications;
        private readonly State testState;
        private readonly Random random;
        private Tuple<string, string> selectedVoice;
        public Languages Languages { get; }

        public NotificationKind NotificationKindThresholdReached
        {
            get { return notificationKind; }
            set
            {
                notificationKind = value;
                SettingsManager.NotificationKindThresholdReached = value;
                OnPropertyChanged();
            }
        }

        public NotificationKind NotificationKindCargoAlmostFull
        {
            get { return notificationKind; }
            set
            {
                notificationKind = value;
                SettingsManager.NotificationKindCargoAlmostFull = value;
                OnPropertyChanged();
            }
        }

        public NotificationKind NotificationKindBlueprintReady
        {
            get { return notificationKind; }
            set
            {
                notificationKind = value;
                SettingsManager.NotificationKindBlueprintReady = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Tuple<string, string>> Voices => testCommanderNotifications.AvailableLanguages();

        public Tuple<string, string> SelectedVoice
        {
            get { return selectedVoice; }
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
            testCommanderNotifications.SubscribeToasts();

            NotificationKindThresholdReached = SettingsManager.NotificationKindThresholdReached;
            NotificationKindCargoAlmostFull = SettingsManager.NotificationKindCargoAlmostFull;
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
                entryData.Threshold = null;
            }

            foreach (var blueprint in testState.Blueprints)
            {
                blueprint.Favorite = false;
            }
        }

        public void TriggerThresholdReached()
        {
            ResetState();
            var entry = testState.Cargo[random.Next(0, testState.Cargo.Count)].Value;
            entry.Threshold = random.Next(10, 30);

            testState.IncrementCargo(entry.Data.Name, random.Next(entry.Threshold.Value, entry.Threshold.Value + 5));
        }

        public void TriggerCargoAlmostFull()
        {
            ResetState();
            if (random.Next(0, 2) == 0)
            {
                var material = testState.Cargo.Values.First(v => v.Data.Kind == Kind.Material);
                testState.IncrementCargo(material.Data.Name, random.Next(995, 1000));
            }
            else
            {
                var data = testState.Cargo.Values.First(v => v.Data.Kind == Kind.Data);
                testState.IncrementCargo(data.Data.Name, random.Next(495, 500));
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
            testCommanderNotifications.UnsubscribeToasts();
        }
    }
}
