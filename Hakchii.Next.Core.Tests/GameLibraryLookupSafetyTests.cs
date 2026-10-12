using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class GameLibraryLookupSafetyTests
{
    [Fact]
    public void Find_NormalizesLookupId()
    {
        var library = new GameLibrary();
        Assert.True(library.Add(new GameEntry { Id = "mario", RomPath = "/games/mario.nes" }).Success);

        Assert.NotNull(library.Find("  MARIO  "));
    }

    [Fact]
    public void Remove_NormalizesLookupId()
    {
        var library = new GameLibrary();
        Assert.True(library.Add(new GameEntry { Id = "mario", RomPath = "/games/mario.nes" }).Success);

        var result = library.Remove("  MARIO  ");

        Assert.True(result.Success);
        Assert.Empty(library.Games);
    }

    [Fact]
    public void PrepareLaunch_NormalizesLookupId()
    {
        var library = new GameLibrary();
        Assert.True(library.Add(new GameEntry
        {
            Id = "mario",
            RomPath = "/games/mario.nes",
            CoreId = "fceumm"
        }).Success);

        var result = library.PrepareLaunch("  MARIO  ", new LaunchCommandBuilder());

        Assert.True(result.Success);
        Assert.Contains("retroarch-clover", library.Find("mario").LaunchCommand);
    }
}
