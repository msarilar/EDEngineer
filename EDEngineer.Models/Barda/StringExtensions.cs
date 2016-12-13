using System.Linq;

namespace EDEngineer.Models.Barda
{
    public static class StringExtensions
    {
        public static string Initials(this string self)
        {
            var initials = self.Replace('-', ' ').Split(' ').Select(s => s[0]);
            return string.Join("", initials);
        }
    }
}