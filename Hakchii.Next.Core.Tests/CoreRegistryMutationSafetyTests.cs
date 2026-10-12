using Hakchii.Next.Core.Emulation;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CoreRegistryMutationSafetyTests
{
    [Fact]
    public void Register_StoresIndependentCoreSnapshot()
    {
        var registry = new CoreRegistry();
        var core = new EmulatorCore
        {
            Id = "fceumm",
            Installed = true,
            Systems = new[] { GameSystem.Nes },
            Command = "/bin/fceumm"
        };

        Assert.True(registry.Register(core).Success);

        core.Id = "mutated";
        core.Command = "/bin/other";
        core.Installed = false;

        var stored = registry.Find("fceumm");
        Assert.NotNull(stored);
        Assert.Equal("fceumm", stored.Id);
        Assert.Equal("/bin/fceumm", stored.Command);
        Assert.True(stored.Installed);
        Assert.Null(registry.Find("mutated"));
    }

    [Fact]
    public void Register_ClonesSupportedSystems()
    {
        var systems = new[] { GameSystem.Nes };
        var registry = new CoreRegistry();
        Assert.True(registry.Register(new EmulatorCore
        {
            Id = "fceumm",
            Installed = true,
            Systems = systems
        }).Success);

        systems[0] = GameSystem.Snes;

        Assert.Single(registry.ForSystem(GameSystem.Nes));
        Assert.Empty(registry.ForSystem(GameSystem.Snes));
    }
}
