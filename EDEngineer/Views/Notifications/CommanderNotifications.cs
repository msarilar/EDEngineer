using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Utils.System;

namespace EDEngineer.Views.Notifications
{
    public class CommanderNotifications : IDisposable
    {
        private readonly State state;
        private readonly BlockingCollection<Notification> notifications = new BlockingCollection<Notification>(); 
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly SpeechSynthesizer speaker;

        private readonly IReadOnlyDictionary<NotificationContentKind, Func<NotificationKind>> settings = new Dictionary
            <NotificationContentKind, Func<NotificationKind>>
        {
            [NotificationContentKind.BlueprintReady] = () => SettingsManager.NotificationKindBlueprintReady
        };

        public CommanderNotifications(State state)
        {
            this.state = state;
            Task.Factory.StartNew(ConsumeNotifications);
            speaker = new SpeechSynthesizer();

            if (string.IsNullOrEmpty(SettingsManager.NotificationVoice))
            {
                SettingsManager.NotificationVoice = AvailableLanguages().First().Item2;
            }
        }

        public IEnumerable<Tuple<string, string>> AvailableLanguages()
        {
            return from voice in speaker.GetInstalledVoices()
            join lang in Languages.Instance.LanguageInfos
                on voice.VoiceInfo.Culture.TwoLetterISOLanguageName equals lang.Value.TwoLetterISOLanguageName
            select Tuple.Create(lang.Value.Name, voice.VoiceInfo.Name);
        }

        public void SetVoice(string voice)
        {
            speaker.SelectVoice(voice);
        }

        private void ConsumeNotifications()
        {
            var toDisplay = new HashSet<Notification>();
            var oneSecond = TimeSpan.FromSeconds(1);
            while (!tokenSource.Token.IsCancellationRequested)
            {
                while (notifications.TryTake(out var item, oneSecond) && toDisplay.Add(item)) ;

                foreach (var group in toDisplay.GroupBy(n => n.ContentKind))
                {
                    var items = AggregateNotifications(@group);

                    foreach (var notification in items)
                    {
                        switch (settings[group.Key]())
                        {
                            case NotificationKind.None:
                                break;
                            case NotificationKind.Toast:
                                ShowToast(notification);
                                break;
                            case NotificationKind.Voice:
                                SpeakVoice(notification);
                                break;
                        }
                    }
                }

                toDisplay.Clear();
            }
        }

        private void SpeakVoice(Notification notification)
        {
            Task.Factory.StartNew(() =>
            {
                SetVoice(SettingsManager.NotificationVoice);
                speaker.Speak(notification.Header);
                speaker.Speak(notification.Content);
            });
        }

        private static void ShowToast(Notification notification)
        {
            try
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

                var stringElements = toastXml.GetElementsByTagName("text");
                stringElements[0].AppendChild(toastXml.CreateTextNode(notification.Header));
                stringElements[1].AppendChild(toastXml.CreateTextNode(notification.Content));

                var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                var imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
            }
            catch (Exception)
            {
                // silently fail for platforms not supporting toasts
            }
        }

        private static IEnumerable<Notification> AggregateNotifications(IGrouping<NotificationContentKind, Notification> @group)
        {
            var items = @group.ToList();

            if (items.Count > 2)
            {
                Notification notification;
                if (@group.Key == NotificationContentKind.BlueprintReady)
                {
                    notification = new Notification(NotificationContentKind.BlueprintReady,
                        Languages.Instance.Translate("Multiple Blueprints Ready"),
                        string.Format(Languages.Instance.Translate("{0} blueprints are available to craft!"),
                            items.Count));
                }
                else
                {
                    notification = items.Last();
                }

                items.Clear();
                items.Add(notification);
            }
            return items;
        }

        private void BlueprintOnFavoriteAvailable(object sender, EventArgs e)
        {
            var blueprint = (Blueprint)sender;
            var translator = Languages.Instance;

            var headerText = translator.Translate("Favorite Blueprint Ready");
            var contentText = $"{translator.Translate(blueprint.Type)} {translator.Translate(blueprint.BlueprintName)} (G{blueprint.Grade})";
            
            notifications.Add(new Notification(NotificationContentKind.BlueprintReady, headerText, contentText));
        }

        public void UnsubscribeNotifications()
        {
            foreach (var blueprint in state.Blueprints)
            {
                blueprint.FavoriteAvailable -= BlueprintOnFavoriteAvailable;
            }
        }

        public void SubscribeNotifications()
        {
            foreach (var blueprint in state.Blueprints)
            {
                blueprint.FavoriteAvailable += BlueprintOnFavoriteAvailable;
            }
        }

        public void Dispose()
        {
            tokenSource.Cancel();
        }
    }
}