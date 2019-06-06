using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ObjectDetection.Models
{
    public class ClassCatalogParser
    {
        private const string CatalogItemPattern =
            @"item {{{0}  name: ""(?<name>.*)""{0}  id: (?<id>\d+){0}  display_name: ""(?<displayName>.*)""{0}}}";

        public IEnumerable<ClassCatalogItem> Parse(string classCatalogPath)
        {
            using (var stream = File.OpenRead(classCatalogPath))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(text))
                {
                    yield break;
                }

                var regex = new Regex(string.Format(CultureInfo.InvariantCulture, CatalogItemPattern,
                    Environment.NewLine));
                var matches = regex.Matches(text);

                if (matches.Count == 0)
                {
                    regex = new Regex(string.Format(CultureInfo.InvariantCulture, CatalogItemPattern, "\n"));
                    matches = regex.Matches(text);
                }

                foreach (Match match in matches)
                {
                    yield return new ClassCatalogItem
                    {
                        Id = int.Parse(match.Groups[2].Value),
                        Name = match.Groups[1].Value,
                        DisplayName = match.Groups[3].Value
                    };
                }
            }
        }
    }
}