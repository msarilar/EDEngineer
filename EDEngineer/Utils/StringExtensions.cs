using System.Linq;

namespace EDEngineer.Utils
{
    public static class StringExtensions
    {
        public static string Initials(this string self)
        {
            var initials = self.Split(' ').Select(s => s[0]);
            return string.Join("", initials);
        }
    }
}