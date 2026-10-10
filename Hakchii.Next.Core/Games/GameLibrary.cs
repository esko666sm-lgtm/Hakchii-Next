using System;
using System.Collections.Generic;

namespace Hakchii.Next.Core.Games
{
    public sealed class GameLibrary : IGameLibrary
    {
        private readonly Dictionary<string,GameEntry> games = new Dictionary<string,GameEntry>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyCollection<GameEntry> Games => games.Values;
        public OperationResult Add(GameEntry game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.Id)) return OperationResult.Fail("Game id is required");
            if (string.IsNullOrWhiteSpace(game.RomPath)) return OperationResult.Fail("ROM path is required");
            if (games.ContainsKey(game.Id)) return OperationResult.Fail("Game already exists: " + game.Id);
            games.Add(game.Id, game); return OperationResult.Ok();
        }
        public GameEntry Find(string id) { games.TryGetValue(id ?? "", out var game); return game; }
    }
}
