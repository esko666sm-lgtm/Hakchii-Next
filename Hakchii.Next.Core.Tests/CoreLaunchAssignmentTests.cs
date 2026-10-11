using Hakchii.Next.Core.Emulation;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CoreLaunchAssignmentTests
{
    [Fact]
    public void Assign_CopiesCoreCommandAndArgumentsToGame()
    {
        var registry = new CoreRegistry();
        registry.Register(new EmulatorCore
        {
            Id = "snes9x2005",
            DisplayName = "Snes9x 2005",
            Installed = true,
            Systems = new[] { GameSystem.Snes },
            Command = "/bin/snes05",
            Arguments = "--rollback-snapshot-period 600"
        });
        var game = new GameEntry
        {
            Id = "smw",
            RomPath = "/games/Super Mario World.sfc",
            System = GameSystem.Snes
        };

        var result = new CoreAssignmentService(registry).Assign(game, "snes9x2005");

        Assert.True(result.Success);
        Assert.Equal("/bin/snes05", game.CoreCommand);
        Assert.Equal("--rollback-snapshot-period 600", game.CoreArguments);
    }

    [Fact]
    public void Assign_CoreWithoutExplicitCommand_FallsBackToRetroArchBuilder()
    {
        var registry = new CoreRegistry();
        registry.Register(new EmulatorCore
        {
            Id = "fceumm",
            DisplayName = "FCEUmm",
            Installed = true,
            Systems = new[] { GameSystem.Nes }
        });
        var game = new GameEntry
        {
            Id = "mario",
            RomPath = "/games/Mario Bros.nes",
            System = GameSystem.Nes
        };

        Assert.True(new CoreAssignmentService(registry).Assign(game).Success);
        var command = new LaunchCommandBuilder().Build(game);

        Assert.Contains("retroarch-clover", command);
        Assert.Contains("fceumm", command);
    }

    [Fact]
    public void Assign_ReplacesPreviousCoreCommand()
    {
        var registry = new CoreRegistry();
        registry.Register(new EmulatorCore
        {
            Id = "fceumm",
            DisplayName = "FCEUmm",
            Installed = true,
            Systems = new[] { GameSystem.Nes }
        });
        var game = new GameEntry
        {
            Id = "mario",
            RomPath = "/games/mario.nes",
            System = GameSystem.Nes,
            CoreCommand = "/bin/old",
            CoreArguments = "--old"
        };

        Assert.True(new CoreAssignmentService(registry).Assign(game, "fceumm").Success);
        Assert.Null(game.CoreCommand);
        Assert.Null(game.CoreArguments);
    }
}
