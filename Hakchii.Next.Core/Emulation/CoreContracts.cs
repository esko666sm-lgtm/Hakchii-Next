using System.Collections.Generic;
using Hakchii.Next.Core.Games;

namespace Hakchii.Next.Core.Emulation
{
    public sealed class EmulatorCore
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public IReadOnlyCollection<GameSystem> Systems { get; set; } = new GameSystem[0];
        public bool Installed { get; set; }
    }
    public interface ICoreRegistry
    {
        IReadOnlyCollection<EmulatorCore> Cores { get; }
        OperationResult Register(EmulatorCore core);
        EmulatorCore Find(string id);
        IReadOnlyCollection<EmulatorCore> ForSystem(GameSystem system, bool installedOnly = true);
    }
    public interface ICoreAssignmentService { OperationResult Assign(GameEntry game, string preferredCoreId = null); }
}
