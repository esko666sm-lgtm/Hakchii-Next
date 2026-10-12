using Hakchii.Next.Core.Emulation;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CoreRegistrySystemSafetyTests
{
    [Fact]
    public void Register_NormalizesSupportedSystems()
    {
        var core = new EmulatorCore
        {
            Id = "fceumm",
            Systems = new[] { GameSystem.Nes, GameSystem.Nes, GameSystem.Unknown },
            Installed = true
        };

        var result = new CoreRegistry().Register(core);

        Assert.True(result.Success);
        Assert.Equal(new[] { GameSystem.Nes }, core.Systems);
    }

    [Fact]
    public void Register_RejectsOnlyUnknownOrUndefinedSystems()
    {
        var registry = new CoreRegistry();

        var unknown = registry.Register(new EmulatorCore
        {
            Id = "unknown",
            Systems = new[] { GameSystem.Unknown }
        });

        var undefined = registry.Register(new EmulatorCore
        {
            Id = "undefined",
            Systems = new[] { (GameSystem)999 }
        });

        Assert.False(unknown.Success);
        Assert.False(undefined.Success);
    }
}
