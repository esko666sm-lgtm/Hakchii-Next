using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hakchii.Next.Core.Cheats
{
    public sealed class RetroArchCheatParser
    {
        public IReadOnlyCollection<CheatEntry> Parse(string text, string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId) || string.IsNullOrWhiteSpace(text))
                return new CheatEntry[0];

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rawLine in text.Replace((char)13, (char)10).Split((char)10))
            {
                var line = rawLine.Trim();
                if (line.Length == 0) continue;

                var equalsIndex = line.IndexOf('=');
                if (equalsIndex <= 0) continue;

                var key = line.Substring(0, equalsIndex).Trim();
                var value = line.Substring(equalsIndex + 1).Trim();
                if (value.Length >= 2 && value[0] == (char)34 && value[value.Length - 1] == (char)34)
                    value = value.Substring(1, value.Length - 2);

                values[key] = Unescape(value);
            }

            int count = 0;
            if (values.TryGetValue("cheats", out var countText))
                int.TryParse(countText, NumberStyles.Integer, CultureInfo.InvariantCulture, out count);

            if (count <= 0)
                count = InferCount(values.Keys);

            var result = new List<CheatEntry>();
            for (int i = 0; i < count; i++)
            {
                if (!values.TryGetValue("cheat" + i + "_code", out var code) || string.IsNullOrWhiteSpace(code))
                    continue;

                values.TryGetValue("cheat" + i + "_desc", out var name);
                values.TryGetValue("cheat" + i + "_enable", out var enabledText);

                result.Add(new CheatEntry
                {
                    Id = gameId + ":" + i.ToString(CultureInfo.InvariantCulture),
                    GameId = gameId,
                    Name = string.IsNullOrWhiteSpace(name) ? "Cheat " + (i + 1).ToString(CultureInfo.InvariantCulture) : name,
                    Code = code,
                    Enabled = string.Equals(enabledText, "true", StringComparison.OrdinalIgnoreCase)
                });
            }

            return result;
        }

        private static int InferCount(IEnumerable<string> keys)
        {
            int max = 0;
            foreach (var key in keys)
            {
                if (!key.StartsWith("cheat", StringComparison.OrdinalIgnoreCase)) continue;
                var underscore = key.IndexOf('_', 5);
                if (underscore <= 5) continue;
                var numberText = key.Substring(5, underscore - 5);
                if (int.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                    max = Math.Max(max, index + 1);
            }
            return max;
        }

        private static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var result = new List<char>(value.Length);
            bool escaping = false;
            foreach (var ch in value)
            {
                if (escaping)
                {
                    result.Add(ch);
                    escaping = false;
                }
                else if (ch == (char)92)
                {
                    escaping = true;
                }
                else
                {
                    result.Add(ch);
                }
            }

            if (escaping) result.Add((char)92);
            return new string(result.ToArray());
        }
    }

    public static class CheatDatabaseRetroArchExtensions
    {
        public static OperationResult ImportRetroArch(this CheatDatabase database, string gameId, string text)
        {
            if (database == null) return OperationResult.Fail("Cheat database is required");
            if (string.IsNullOrWhiteSpace(gameId)) return OperationResult.Fail("Game id is required");

            var parsed = new RetroArchCheatParser().Parse(text, gameId);
            if (parsed.Count == 0) return OperationResult.Fail("No valid cheats found");

            foreach (var cheat in parsed)
            {
                var result = database.Add(cheat);
                if (!result.Success) return result;
            }

            return OperationResult.Ok();
        }
    }
}
