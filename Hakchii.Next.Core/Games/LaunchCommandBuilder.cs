using System;

namespace Hakchii.Next.Core.Games
{
    public interface ILaunchCommandBuilder { string Build(GameEntry game); }
    public sealed class LaunchCommandBuilder : ILaunchCommandBuilder
    {
        public string Build(GameEntry game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            if (string.IsNullOrWhiteSpace(game.RomPath)) throw new ArgumentException("ROM path is required");
            if (!string.IsNullOrWhiteSpace(game.CoreCommand))
            {
                var command = game.CoreCommand.Trim() + " " + Quote(game.RomPath);
                if (!string.IsNullOrWhiteSpace(game.CoreArguments))
                    command += " " + game.CoreArguments.Trim();
                return command;
            }
            if (string.IsNullOrWhiteSpace(game.CoreId)) return Quote(game.RomPath);
            return "/bin/retroarch-clover " + Quote(game.RomPath) + " --core " + Quote(game.CoreId.Trim());
        }
        private static string Quote(string value)
        {
            var escaped = value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("$", "\\$")
                .Replace("`", "\\`");

            return "\"" + escaped + "\"";
        }
    }
}
