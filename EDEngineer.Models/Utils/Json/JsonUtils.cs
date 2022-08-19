using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EDEngineer.Models.Utils.Json
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
                                                  .Where(v => v is JValue || v is JArray)
                                                  .Select(v => v is JValue ? $"\"{(string) v}\"" : $"\"{string.Join(", ", ((JArray) v).ToList())}\"")
                                                  .FirstOrDefault())
                                    .Reverse()
                                    .SkipWhile(s => s == null)
                                    .Reverse());
            
            var csvRows = new[] { columns }.Concat(rows).Select(r => string.Join(",", r));
            var csv = string.Join(Environment.NewLine, csvRows);

            return csv;
        }
    }
}
