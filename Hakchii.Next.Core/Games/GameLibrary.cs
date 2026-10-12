using System;
using System.Collections.Generic;

namespace Hakchii.Next.Core.Games
{
    public sealed class GameLibrary : IGameLibrary
    {
        private readonly Dictionary<string, GameEntry> games =
            new Dictionary<string, GameEntry>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<GameEntry> Games => games.Values;

        public OperationResult Add(GameEntry game)
        {
            var validation = Validate(game);
            if (!validation.Success) return validation;
            if (games.ContainsKey(game.Id)) return OperationResult.Fail("Game already exists: " + game.Id);

            games.Add(game.Id, game);
            return OperationResult.Ok();
        }

        public OperationResult Update(GameEntry game)
        {
            var validation = Validate(game);
            if (!validation.Success) return validation;
            if (!games.ContainsKey(game.Id)) return OperationResult.Fail("Game does not exist: " + game.Id);

            games[game.Id] = game;
            return OperationResult.Ok();
        }

        public OperationResult Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return OperationResult.Fail("Game id is required");
            if (!games.Remove(id)) return OperationResult.Fail("Game does not exist: " + id);
            return OperationResult.Ok();
        }

        public OperationResult PrepareLaunch(string id, ILaunchCommandBuilder builder)
        {
            if (builder == null) return OperationResult.Fail("Launch command builder is required");

            var game = Find(id);
            if (game == null) return OperationResult.Fail("Game does not exist: " + id);

            try
            {
                var command = builder.Build(game);
                if (string.IsNullOrWhiteSpace(command))
                    return OperationResult.Fail("Launch command is required");

                game.LaunchCommand = command.Trim();
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message);
            }
        }

        public GameEntry Find(string id)
        {
            games.TryGetValue(id ?? string.Empty, out var game);
            return game;
        }

        private static OperationResult Validate(GameEntry game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.Id))
                return OperationResult.Fail("Game id is required");
            if (string.IsNullOrWhiteSpace(game.RomPath))
                return OperationResult.Fail("ROM path is required");

            game.Id = game.Id.Trim();
            game.RomPath = game.RomPath.Trim();
            return OperationResult.Ok();
        }
    }
}
