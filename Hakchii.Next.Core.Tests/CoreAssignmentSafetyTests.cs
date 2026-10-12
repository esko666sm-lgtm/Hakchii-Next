using System;
using Hakchii.Next.Core.Emulation;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CoreAssignmentSafetyTests
{
    [Fact]
    public void RegistryFind_NormalizesLookupId()
    {
        var registry = new CoreRegistry();
        Assert.True(registry.Register(new EmulatorCore
        {
            Id = "fceumm",
            Installed = true,
            Systems = new[] { GameSystem.Nes }
        }).Success);

        Assert.NotNull(registry.Find("  FCEUMM  "));
    }

    [Fact]
    public void Assign_NormalizesPreferredCoreId()
    {
        var registry = new CoreRegistry();
        Assert.True(registry.Register(new EmulatorCore
        {
            Id = "fceumm",
            Installed = true,
            Systems = new[] { GameSystem.Nes }
        }).Success);

        var game = new GameEntry
        {
            Id = "mario",
            RomPath = "/games/mario.nes",
            System = GameSystem.Nes
        };

        var result = new CoreAssignmentService(registry).Assign(game, "  FCEUMM  ");

        Assert.True(result.Success);
        Assert.Equal("fceumm", game.CoreId);
    }

    [Fact]
    public void AssignmentService_RejectsNullRegistry()
    {
        Assert.Throws<ArgumentNullException>(() => new CoreAssignmentService(null!));
    }
}
