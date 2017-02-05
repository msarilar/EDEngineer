using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Windows.UI.Notifications;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Utils.System;
using EDEngineer.Views.Popups;

public class CommanderToasts
{
    private readonly State state;
    private readonly string commanderName;

    public CommanderToasts(State state, string commanderName)
    {
        this.state = state;
        this.commanderName = commanderName;
    }

    private void ThresholdToastCheck(string item)
    {
        var translator = Languages.Instance;

        if (!state.Cargo.ContainsKey(item))
        {
            return;
        }

        var entry = state.Cargo[item];

        if (entry.Threshold.HasValue && entry.Threshold <= entry.Count)
        {
            try
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

                var stringElements = toastXml.GetElementsByTagName("text");
                var content = string.Format(
                    translator.Translate(
                        "Reached {0} {1} - threshold set at {2} (click this to configure your thresholds)"),
                    entry.Count, translator.Translate(entry.Data.Name), entry.Threshold);
                stringElements[0].AppendChild(toastXml.CreateTextNode(translator.Translate("Threshold Reached!")));
                stringElements[1].AppendChild(toastXml.CreateTextNode(content));

                var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                var imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                var toast = new ToastNotification(toastXml);
                toast.Activated +=
                    (o, e) => Application.Current.Dispatcher.Invoke(() => ThresholdsManagerWindow.ShowThresholds(translator, state.Cargo, commanderName));

                ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
            }
            catch (Exception)
            {
                // silently fail for platforms not supporting toasts
            }
        }
    }

    private void LimitToastCheck(string property)
    {
        if (!SettingsManager.CargoAlmostFullWarningEnabled)
        {
            return;
        }

        var translator = Languages.Instance;

        var ratio = state.MaxMaterials - state.MaterialsCount;
        string headerText, contentText;
        if (ratio <= 5 && property == "MaterialsCount")
        {
            headerText = translator.Translate("Materials Almost Full!");
            contentText = string.Format(translator.Translate("You have only {0} slots left for your materials."), ratio);
        }
        else if ((ratio = state.MaxData - state.DataCount) <= 5 && property == "DataCount")
        {
            headerText = translator.Translate("Data Almost Full!");
            contentText = string.Format(translator.Translate("You have only {0} slots left for your data."), ratio);
        }
        else
        {
            return;
        }

        try
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            var stringElements = toastXml.GetElementsByTagName("text");

            stringElements[0].AppendChild(toastXml.CreateTextNode(headerText));
            stringElements[1].AppendChild(toastXml.CreateTextNode(contentText));

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

    private void BlueprintOnFavoriteAvailable(object sender, EventArgs e)
    {
        if (!SettingsManager.BlueprintReadyToastEnabled)
        {
            return;
        }

        var blueprint = (Blueprint)sender;
        try
        {
            var translator = Languages.Instance;

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            var stringElements = toastXml.GetElementsByTagName("text");

            stringElements[0].AppendChild(toastXml.CreateTextNode(translator.Translate("Blueprint Ready")));
            stringElements[1].AppendChild(toastXml.CreateTextNode($"{translator.Translate(blueprint.BlueprintName)} (G{blueprint.Grade})"));
            stringElements[2].AppendChild(toastXml.CreateTextNode($"{string.Join(", ", blueprint.Engineers)}"));

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

    public void UnsubscribeToasts()
    {
        if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)) // windows 8 or more recent
        {
            foreach (var blueprint in state.Blueprints)
            {
                blueprint.FavoriteAvailable -= BlueprintOnFavoriteAvailable;
            }

            state.PropertyChanged -= StateCargoCountChanged;
        }
    }

    public void SubscribeToasts()
    {
        if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)) // windows 8 or more recent
        {
            foreach (var blueprint in state.Blueprints)
            {
                blueprint.FavoriteAvailable += BlueprintOnFavoriteAvailable;
            }

            state.PropertyChanged += StateCargoCountChanged;
        }
    }

    private void StateCargoCountChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "MaterialsCount" || e.PropertyName == "DataCount")
        {
            LimitToastCheck(e.PropertyName);
        }

        ThresholdToastCheck(e.PropertyName);
    }
}