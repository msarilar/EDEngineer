using System;
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
        public static void ShowReleaseNotes(string newVersionString)
        {
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
            foreach (var release in releases)
            {
                var releaseVersion = Version.Parse((string)release["tag_name"]);

                if (releaseVersion <= newVersion)
                {
                    builder.AppendLine($"Version : {releaseVersion}");
                    builder.AppendLine((string) release["body"]);
                    builder.AppendLine("--------");
                    builder.AppendLine();
                }
            }

            if (builder.Length > 0)
            {
                MessageBox.Show(builder.ToString(), $"Release notes (new version: {newVersion})", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
