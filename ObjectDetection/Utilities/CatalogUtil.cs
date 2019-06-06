using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ObjectDetection.Models;

namespace ObjectDetection.Utilities
{
    public static class CatalogUtil
    {
        private static readonly string CATALOG_ITEM_PATTERN =
            @"item {{{0}  name: ""(?<name>.*)""{0}  id: (?<id>\d+){0}  display_name: ""(?<displayName>.*)""{0}}}";
        private static readonly string CATALOG_ITEM_PATTERN_ENV =
            string.Format(CultureInfo.InvariantCulture, CATALOG_ITEM_PATTERN, Environment.NewLine);
        private static readonly string CATALOG_ITEM_PATTERN_UNIX =
            string.Format(CultureInfo.InvariantCulture, CATALOG_ITEM_PATTERN, "\n");

        public static IEnumerable<CatalogItem> ReadCatalogItems(string file)
        {
            using (var stream = File.OpenRead(file))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(text))
                {
                    yield break;
                }

                var regex = new Regex(CATALOG_ITEM_PATTERN_ENV);
                var matches = regex.Matches(text);

                if (matches.Count == 0)
                {
                    regex = new Regex(CATALOG_ITEM_PATTERN_UNIX);
                    matches = regex.Matches(text);
                }

                foreach (Match match in matches)
                {
                    yield return new CatalogItem
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