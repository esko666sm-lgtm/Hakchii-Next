using Hakchii.Next.Core;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class GameLaunchPreparationSafetyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PrepareLaunch_RejectsEmptyBuiltCommand(string? command)
    {
        var library = CreateLibrary();
        var result = library.PrepareLaunch("mario", new Builder(command));

        Assert.False(result.Success);
        Assert.Null(library.Find("mario").LaunchCommand);
    }

    [Fact]
    public void PrepareLaunch_FailedRebuild_PreservesExistingCommand()
    {
        var library = CreateLibrary();
        Assert.True(library.PrepareLaunch("mario", new Builder("/bin/retroarch mario.nes")).Success);

        var result = library.PrepareLaunch("mario", new Builder("   "));

        Assert.False(result.Success);
        Assert.Equal("/bin/retroarch mario.nes", library.Find("mario").LaunchCommand);
    }

    [Fact]
    public void PrepareLaunch_ValidCommand_TrimsOuterWhitespace()
    {
        var library = CreateLibrary();

        var result = library.PrepareLaunch("mario", new Builder("  /bin/retroarch mario.nes  "));

        Assert.True(result.Success);
        Assert.Equal("/bin/retroarch mario.nes", library.Find("mario").LaunchCommand);
    }

    private static GameLibrary CreateLibrary()
    {
        var library = new GameLibrary();
        Assert.True(library.Add(new GameEntry
        {
            Id = "mario",
            RomPath = "mario.nes"
        }).Success);
        return library;
    }

    private sealed class Builder : ILaunchCommandBuilder
    {
        private readonly string command;
        public Builder(string? command) { this.command = command!; }
        public string Build(GameEntry game) => command;
    }
}
