using System.Linq;

namespace EDEngineer.Models.Utils
{
    public static class StringExtensions
    {
        public static string Initials(this string self)
        {
            var initials = self.Replace('-', ' ').Split(' ').Where(s => s.Length > 0).Select(s => s[0]);
            return string.Join("", initials);
        }
    }
}