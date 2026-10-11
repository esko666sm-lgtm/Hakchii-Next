using System;
using System.Collections.Generic;
using System.Linq;
using Hakchii.Next.Core.Games;

namespace Hakchii.Next.Core.Emulation
{
    public sealed class CoreRegistry : ICoreRegistry
    {
        private readonly Dictionary<string,EmulatorCore> cores = new Dictionary<string,EmulatorCore>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyCollection<EmulatorCore> Cores => cores.Values;
        public OperationResult Register(EmulatorCore core)
        {
            if (core == null || string.IsNullOrWhiteSpace(core.Id)) return OperationResult.Fail("Core id is required");
            if (cores.ContainsKey(core.Id)) return OperationResult.Fail("Core already registered: " + core.Id);
            cores.Add(core.Id,core); return OperationResult.Ok();
        }
        public EmulatorCore Find(string id) { cores.TryGetValue(id ?? "", out var core); return core; }
        public IReadOnlyCollection<EmulatorCore> ForSystem(GameSystem system, bool installedOnly = true) => cores.Values.Where(c => (!installedOnly || c.Installed) && (c.Systems?.Contains(system) ?? false)).ToArray();
    }

    public sealed class CoreAssignmentService : ICoreAssignmentService
    {
        private readonly ICoreRegistry registry;
        public CoreAssignmentService(ICoreRegistry registry) { this.registry = registry; }
        public OperationResult Assign(GameEntry game, string preferredCoreId = null)
        {
            if (game == null) return OperationResult.Fail("Game is required");
            EmulatorCore core = null;
            if (!string.IsNullOrWhiteSpace(preferredCoreId))
            {
                core = registry.Find(preferredCoreId);
                if (core == null || !core.Installed) return OperationResult.Fail("Preferred core is not installed: " + preferredCoreId);
                if (!(core.Systems?.Contains(game.System) ?? false)) return OperationResult.Fail("Preferred core is incompatible with game system");
            }
            else core = registry.ForSystem(game.System).FirstOrDefault();
            if (core == null) return OperationResult.Fail("No compatible installed core found");
            game.CoreId = core.Id;
            game.CoreCommand = string.IsNullOrWhiteSpace(core.Command) ? null : core.Command;
            game.CoreArguments = string.IsNullOrWhiteSpace(core.Arguments) ? null : core.Arguments;
            return OperationResult.Ok();
        }
    }
}
