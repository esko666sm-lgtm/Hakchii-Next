using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class LaunchCommandQuotingSafetyTests
{
    [Fact]
    public void Build_EscapesShellExpansionCharactersInRomPath()
    {
        var command = new LaunchCommandBuilder().Build(new GameEntry
        {
            Id = "test",
            RomPath = "/games/$HOME`demo`.nes",
            CoreId = "fceumm"
        });

        Assert.Contains("\\$HOME", command);
        Assert.Contains("\\`demo\\`", command);
    }

    [Fact]
    public void Build_TrimsExplicitCoreCommandAndCoreId()
    {
        var explicitCommand = new LaunchCommandBuilder().Build(new GameEntry
        {
            Id = "snes",
            RomPath = "/games/game.sfc",
            CoreCommand = "  /bin/snes05  "
        });

        var retroArchCommand = new LaunchCommandBuilder().Build(new GameEntry
        {
            Id = "nes",
            RomPath = "/games/game.nes",
            CoreId = "  fceumm  "
        });

        Assert.StartsWith("/bin/snes05 ", explicitCommand);
        Assert.Contains("--core \"fceumm\"", retroArchCommand);
    }
}
