using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EDEngineer.Models.Utils
{
    public static class IO
    {
        private static readonly Dictionary<char, string> specialCharactersMapping = new Dictionary<char, string>
        {
            ['<'] = "&#60;",
            ['>'] = "&#62;",
            [':'] = "&#58;",
            ['"'] = "&ldquo;",
            ['/'] = "&#47;",
            ['\\'] = "&#92;",
            ['|'] = "&#124;",
            ['?'] = "&#63;",
            ['*'] = "&#42;",
        };

        private static readonly Dictionary<string, string> specialCharactersMappingReversed =
            specialCharactersMapping.ToDictionary(k => k.Value, k => k.Key.ToString());

        private static string SanitizeChar(char input)
        {
            if (!specialCharactersMapping.TryGetValue(input, out var output))
            {
                return input.ToString();
            }

            return output;
        }

        public static string Sanitize(this string input)
        {
            return input.Select(name => name)
                        .Aggregate(string.Empty, (acc, c) => acc + SanitizeChar(c));
        }

        public static string Desanitize(this string input)
        {
            specialCharactersMappingReversed.Keys.ToList().ForEach(k =>
            {
                input = input.Replace(k,
                    specialCharactersMappingReversed[k]);
            });
            return input;
        }

        static IO()
        {
            blueprintsJson = ReadResource("blueprints");
            releaseNotesJson = ReadResource("releaseNotes");
            localizationJson = ReadResource("localization");
            entryDatasJson = ReadResource("entryData");
        }

        public static string ReadResource(string resource)
        {
            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream($"EDEngineer.Resources.Data.{resource}.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static readonly string blueprintsJson;
        private static readonly string releaseNotesJson;
        private static readonly string localizationJson;
        private static readonly string entryDatasJson;

        public static string GetBlueprintsJson()
        {
            return blueprintsJson;
        }

        public static string GetReleaseNotesJson()
        {
            return releaseNotesJson;
        }

        public static string GetLocalizationJson()
        {
            return localizationJson;
        }

        public static string GetEntryDatasJson()
        {
            return entryDatasJson;
        }

        public static string GetManualChangesDirectory()
        {
            string directory;

            var localDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EDEngineer");
            var roamingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EDEngineer");

            if (Directory.Exists(roamingDirectory))
            {
                directory = roamingDirectory;
            }
            else if (Directory.Exists(localDirectory) && Directory.GetFiles(localDirectory).Any(f => f != null && Path.GetFileName(f).StartsWith("manualChanges.") && f.EndsWith(".json")))
            {
                directory = localDirectory;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(roamingDirectory);
                    directory = roamingDirectory;
                }
                catch
                {
                    Directory.CreateDirectory(localDirectory);
                    directory = localDirectory;
                }
            }

            return directory;
        }
    }
}
