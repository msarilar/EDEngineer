using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models;
using Newtonsoft.Json;

namespace EDEngineer.Utils.System
{
    public static class ReleaseNotesManager
    {
        public static void ShowReleaseNotes(string oldVersionString, string newVersionString)
        {
            Version oldVersion;
            if (!Version.TryParse(oldVersionString, out oldVersion))
            {
                oldVersion = new Version(1, 0, 0, 0);
            }

            var newVersion = Version.Parse(newVersionString);

            var releaseNotes = JsonConvert.DeserializeObject<List<ReleaseNote>>(IOManager.GetReleaseNotesJson());

            var list =  releaseNotes.ToList();
            if (list.Any())
            {
                new ReleaseNotesWindow(list, $"Release notes (new version: {newVersion}, old version: {oldVersion})").ShowDialog();
            }
        }
    }
}
