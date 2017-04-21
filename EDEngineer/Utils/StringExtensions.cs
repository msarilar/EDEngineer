using System;
using System.ComponentModel;
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

        public static string GetLabel(this Enum en)
        {
            var type = en.GetType();

            var memInfo = type.GetMember(en.ToString());
            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }
    }
}