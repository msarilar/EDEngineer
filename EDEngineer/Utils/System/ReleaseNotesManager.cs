using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            string releasesJson = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "EDEngineer");
                var response = client.GetAsync("https://api.github.com/repos/msarilar/EDEngineer/releases").Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    releasesJson = response.Content.ReadAsStringAsync().Result;
                }
            }

            if (releasesJson == null)
            {
                return;
            }

            var releases = (JArray)JsonConvert.DeserializeObject(releasesJson);

            var builder = new StringBuilder();

            var releaseNotes =
                from release in releases
                let releaseVersionString = (string)release["tag_name"]
                let releaseVersion = Version.Parse(releaseVersionString)
                orderby releaseVersion descending
                where releaseVersion <= newVersion && releaseVersion > oldVersion
                select Tuple.Create(releaseVersionString, (string) release["body"]);

            var list =  releaseNotes.ToList();
            if (list.Any())
            {
                new ReleaseNotesWindow(list, $"Release notes (new version: {newVersion}, old version: {oldVersion})").ShowDialog();
            }
        }
    }
}
