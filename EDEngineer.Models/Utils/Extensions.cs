using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EDEngineer.Models.Utils
{
    public static class Extensions
    {
        public static string Initials(this string self)
        {
            var initials = self.Replace('-', ' ').Replace('(', ' ').Replace(')', ' ').Split(' ').Where(s => s.Length > 0).Select(s => s[0]);
            return string.Join("", initials);
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            TKey key,
            Func<TKey, TValue> adder)
        {
            if (!dict.TryGetValue(key, out var result))
            {
                dict[key] = result = adder(key);
            }

            return result;
        }

        public static bool IsIn(this string target, string field)
        {
            return field.ToLowerInvariant().Replace(" ", "").Replace("_", "")
                 .Contains(target.ToLowerInvariant().Replace(" ", "").Replace("_", ""));
        }

        public static TResult? Map<TInput, TResult>(this TInput? o, Func<TInput, TResult> evaluator)
            where TInput : struct
            where TResult : struct
        {
            if (!o.HasValue)
            {
                return default(TResult?);
            }

            return evaluator(o.Value);
        }

        public static string Description(this Enum value)
        {
            if (value == null)
            {
                return null;
            }

            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
                return null;
            }

            var attribute = (DescriptionAttribute)fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString("G");
        }

        public static int MaximumCapacity(this Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.VeryCommon:
                    return 300;
                case Rarity.Common:
                    return 250;
                case Rarity.Standard:
                    return 200;
                case Rarity.Rare:
                    return 150;
                case Rarity.VeryRare:
                    return 100;
                default:
                    return 300;
            }
        }

        public static string ToReadable(this string str)
        {
            if (str == null)
            {
                return null;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '_')
                {
                    builder.Append(" ");
                    continue;
                }

                if (char.IsUpper(str[i])) 
                {
                    if (i == 0 && !char.IsUpper(str[i + 1]))
                    {
                        builder.Append(" ");
                    }
                    else if (i > 0 && str[i - 1] != ' ' && !char.IsUpper(str[i - 1]))
                    {
                        builder.Append(" ");
                    }
                }

                builder.Append(str[i]);
            }

            return builder.ToString();
        }
    }
}