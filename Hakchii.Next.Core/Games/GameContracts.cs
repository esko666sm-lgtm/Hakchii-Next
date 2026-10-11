using System;
using System.Collections.Generic;

namespace Hakchii.Next.Core.Games
{
    public enum GameSystem { Unknown, Nes, Snes }
    public sealed class GameEntry
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string RomPath { get; set; }
        public GameSystem System { get; set; }
        public string CoreId { get; set; }
        public string CoreCommand { get; set; }
        public string CoreArguments { get; set; }
        public string LaunchCommand { get; set; }
        public IDictionary<string,string> Metadata { get; set; } = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    }
    public interface IGameLibrary
    {
        IReadOnlyCollection<GameEntry> Games { get; }
        OperationResult Add(GameEntry game);
        OperationResult Update(GameEntry game);
        OperationResult Remove(string id);
        OperationResult PrepareLaunch(string id, ILaunchCommandBuilder builder);
        GameEntry Find(string id);
    }
}
