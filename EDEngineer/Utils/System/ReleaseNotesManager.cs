using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models;
using EDEngineer.Models.Utils;
using Newtonsoft.Json;

namespace EDEngineer.Utils.System
{
    public static class ReleaseNotesManager
    {
        public static bool RequireReset { get; set; }

        public static void ShowReleaseNotesIfNecessary()
        {
            var oldVersionString = Properties.Settings.Default.CurrentVersion;
            var newVersionString = Properties.Settings.Default.Version;

            if (oldVersionString == newVersionString)
            {
                return;
            }

            Properties.Settings.Default.CurrentVersion = newVersionString;

            Properties.Settings.Default.Save();

            if (!Version.TryParse(oldVersionString, out var oldVersion))
            {
                oldVersion = new Version(1, 0, 0, 0);
            }
            else if (oldVersion < new Version(1, 0, 0, 27))
            {
                Properties.Settings.Default.ResetUI = true;
            }

            var newVersion = Version.Parse(newVersionString);

            ShowReleaseNotes($"Release notes (new version: {newVersion}, old version: {oldVersion})");
        }

        public static void ShowReleaseNotes(string title = "Release Notes")
        {
            var releaseNotes = JsonConvert.DeserializeObject<List<ReleaseNote>>(Helpers.GetReleaseNotesJson());
            RequireReset = releaseNotes.FirstOrDefault()?.Reset == true;

            var list = releaseNotes.ToList();
            if (list.Any())
            {
                new Views.Popups.ReleaseNotesWindow(list, title).ShowDialog();
            }
        }
    }
}
