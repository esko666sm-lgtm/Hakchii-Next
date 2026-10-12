using System;
using System.Collections.Generic;
using System.Linq;
using Hakchii.Next.Core.Games;

namespace Hakchii.Next.Core.Cheats
{
    public sealed class CheatEntry
    {
        public string Id { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Enabled { get; set; }
    }

    public interface ICheatProvider
    {
        IReadOnlyCollection<CheatEntry> Find(GameEntry game);
    }

    public sealed class CheatDatabase : ICheatProvider
    {
        private readonly List<CheatEntry> items = new List<CheatEntry>();

        public OperationResult Add(CheatEntry cheat)
        {
            if (cheat == null ||
                string.IsNullOrWhiteSpace(cheat.Id) ||
                string.IsNullOrWhiteSpace(cheat.GameId) ||
                string.IsNullOrWhiteSpace(cheat.Code))
                return OperationResult.Fail("Invalid cheat");

            cheat.Id = cheat.Id.Trim();
            cheat.GameId = cheat.GameId.Trim();
            cheat.Code = cheat.Code.Trim();
            cheat.Name = string.IsNullOrWhiteSpace(cheat.Name) ? null : cheat.Name.Trim();

            if (items.Any(x =>
                string.Equals(x.GameId, cheat.GameId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Id, cheat.Id, StringComparison.OrdinalIgnoreCase)))
                return OperationResult.Fail("Cheat already exists for game: " + cheat.Id);

            items.Add(new CheatEntry
            {
                Id = cheat.Id,
                GameId = cheat.GameId,
                Name = cheat.Name,
                Code = cheat.Code,
                Enabled = cheat.Enabled
            });
            return OperationResult.Ok();
        }

        public IReadOnlyCollection<CheatEntry> Find(GameEntry game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.Id))
                return new CheatEntry[0];

            var gameId = game.Id.Trim();
            return items
                .Where(x => string.Equals(x.GameId, gameId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    public sealed class RetroArchCheatSerializer
    {
        public string Serialize(IEnumerable<CheatEntry> cheats)
        {
            var items = (cheats ?? new CheatEntry[0]).ToArray();
            for (var i = 0; i < items.Length; i++)
            {
                var cheat = items[i];
                if (cheat == null)
                    throw new ArgumentException("Cheat entry is required", nameof(cheats));

                cheat.Name = string.IsNullOrWhiteSpace(cheat.Name) ? null : cheat.Name.Trim();
                cheat.Code = string.IsNullOrWhiteSpace(cheat.Code) ? string.Empty : cheat.Code.Trim();
            }

            var lines = new List<string> { "cheats = \"" + items.Length + "\"" };

            for (var i = 0; i < items.Length; i++)
            {
                lines.Add($"cheat{i}_desc = \"{Escape(items[i].Name)}\"");
                lines.Add($"cheat{i}_code = \"{Escape(items[i].Code)}\"");
                lines.Add($"cheat{i}_enable = \"{items[i].Enabled.ToString().ToLowerInvariant()}\"");
            }

            return string.Join("\n", lines);
        }

        private static string Escape(string value)
            => (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
    }
}
