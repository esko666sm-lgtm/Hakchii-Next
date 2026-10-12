using Hakchii.Next.Core.Emulation;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CoreRegistrySafetyTests
{
    [Fact]
    public void Register_RejectsCoreWithoutSupportedSystems()
    {
        var registry = new CoreRegistry();

        var result = registry.Register(new EmulatorCore
        {
            Id = "dead-core",
            Installed = true,
            Systems = new GameSystem[0]
        });

        Assert.False(result.Success);
        Assert.Null(registry.Find("dead-core"));
    }

    [Fact]
    public void Register_RejectsArgumentsWithoutExplicitCommand()
    {
        var registry = new CoreRegistry();

        var result = registry.Register(new EmulatorCore
        {
            Id = "bad-core",
            Installed = true,
            Systems = new[] { GameSystem.Nes },
            Arguments = "--foo"
        });

        Assert.False(result.Success);
        Assert.Null(registry.Find("bad-core"));
    }

    [Fact]
    public void Register_NormalizesIdAndCommandMetadata()
    {
        var registry = new CoreRegistry();

        var result = registry.Register(new EmulatorCore
        {
            Id = "  snes9x  ",
            Installed = true,
            Systems = new[] { GameSystem.Snes },
            Command = "  /bin/snes9x  ",
            Arguments = "  --fast  "
        });

        Assert.True(result.Success);
        var core = registry.Find("snes9x");
        Assert.NotNull(core);
        Assert.Equal("snes9x", core.Id);
        Assert.Equal("/bin/snes9x", core.Command);
        Assert.Equal("--fast", core.Arguments);
    }
}
