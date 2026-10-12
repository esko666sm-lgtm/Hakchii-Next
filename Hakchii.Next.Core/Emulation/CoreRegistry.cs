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
            if (core.Systems == null || core.Systems.Count == 0) return OperationResult.Fail("Core must support at least one system");

            core.Systems = core.Systems
                .Where(system => system != GameSystem.Unknown && Enum.IsDefined(typeof(GameSystem), system))
                .Distinct()
                .ToArray();
            if (core.Systems.Count == 0) return OperationResult.Fail("Core must support at least one valid system");

            core.Id = core.Id.Trim();
            core.Command = string.IsNullOrWhiteSpace(core.Command) ? null : core.Command.Trim();
            core.Arguments = string.IsNullOrWhiteSpace(core.Arguments) ? null : core.Arguments.Trim();

            if (core.Command == null && core.Arguments != null)
                return OperationResult.Fail("Core arguments require an explicit command");
            if (cores.ContainsKey(core.Id)) return OperationResult.Fail("Core already registered: " + core.Id);

            var snapshot = new EmulatorCore
            {
                Id = core.Id,
                DisplayName = core.DisplayName,
                Systems = core.Systems.ToArray(),
                Installed = core.Installed,
                Command = core.Command,
                Arguments = core.Arguments
            };

            cores.Add(snapshot.Id, snapshot);
            return OperationResult.Ok();
        }
        public EmulatorCore Find(string id)
        {
            var normalizedId = string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
            cores.TryGetValue(normalizedId, out var core);
            return core;
        }
        public IReadOnlyCollection<EmulatorCore> ForSystem(GameSystem system, bool installedOnly = true) => cores.Values.Where(c => (!installedOnly || c.Installed) && (c.Systems?.Contains(system) ?? false)).ToArray();
    }

    public sealed class CoreAssignmentService : ICoreAssignmentService
    {
        private readonly ICoreRegistry registry;
        public CoreAssignmentService(ICoreRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }
        public OperationResult Assign(GameEntry game, string preferredCoreId = null)
        {
            if (game == null) return OperationResult.Fail("Game is required");
            EmulatorCore core = null;
            if (!string.IsNullOrWhiteSpace(preferredCoreId))
            {
                var normalizedPreferredCoreId = preferredCoreId.Trim();
                core = registry.Find(normalizedPreferredCoreId);
                if (core == null || !core.Installed) return OperationResult.Fail("Preferred core is not installed: " + normalizedPreferredCoreId);
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
