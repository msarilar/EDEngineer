using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EDEngineer.Models.Barda.Collections
{
    public static class JsonUtils
    {
        public static string ToCsv(string json)
        {
            var obj = JObject.Parse(json);
            
            var values = obj.DescendantsAndSelf()
                            .OfType<JProperty>()
                            .Where(p => p.Value is JValue)
                            .GroupBy(p => p.Name)
                            .ToList();

            var columns = values.Select(g => g.Key).ToArray();
            
            var parentsWithChildren =
                values.SelectMany(g => g).SelectMany(v => v.AncestorsAndSelf().OfType<JObject>().Skip(1)).ToHashSet();

            var rows = obj
                .DescendantsAndSelf()
                .OfType<JObject>()
                .Where(o => o.PropertyValues().OfType<JValue>().Any())
                .Where(o => o == obj || !parentsWithChildren.Contains(o))
                .Select(o => columns.Select(c => o.AncestorsAndSelf()
                                                  .OfType<JObject>()
                                                  .Select(parent => parent[c])
                                                  .Where(v => v is JValue)
                                                  .Select(v => (string) v)
                                                  .FirstOrDefault())
                                    .Reverse()
                                    .SkipWhile(s => s == null)
                                    .Reverse());
            
            var csvRows = new[] { columns }.Concat(rows).Select(r => string.Join(",", r));
            var csv = string.Join(Environment.NewLine, csvRows);

            return csv;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

    }
}
