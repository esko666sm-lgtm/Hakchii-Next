using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class GameLibraryMutationTests
{
    [Fact]
    public void Update_ReplacesExistingGameById()
    {
        var library = new GameLibrary();
        library.Add(new GameEntry { Id = "mario", Title = "Mario", RomPath = "/games/mario.nes" });

        var result = library.Update(new GameEntry
        {
            Id = "mario",
            Title = "Super Mario Bros.",
            RomPath = "/games/smb.nes"
        });

        Assert.True(result.Success);
        Assert.Equal("Super Mario Bros.", library.Find("MARIO").Title);
        Assert.Equal("/games/smb.nes", library.Find("mario").RomPath);
    }

    [Fact]
    public void Update_UnknownGame_Fails()
    {
        var library = new GameLibrary();
        var result = library.Update(new GameEntry { Id = "missing", RomPath = "/games/x.nes" });
        Assert.False(result.Success);
    }

    [Fact]
    public void Remove_DeletesExistingGame()
    {
        var library = new GameLibrary();
        library.Add(new GameEntry { Id = "mario", RomPath = "/games/mario.nes" });

        var result = library.Remove("MARIO");

        Assert.True(result.Success);
        Assert.Null(library.Find("mario"));
    }

    [Fact]
    public void PrepareLaunch_WritesComputedCommand()
    {
        var library = new GameLibrary();
        var game = new GameEntry
        {
            Id = "mario",
            RomPath = "/games/Mario Bros.nes",
            CoreId = "fceumm"
        };
        library.Add(game);

        var result = library.PrepareLaunch("mario", new LaunchCommandBuilder());

        Assert.True(result.Success);
        Assert.Contains("retroarch-clover", game.LaunchCommand);
        Assert.Contains("fceumm", game.LaunchCommand);
    }

    [Fact]
    public void PrepareLaunch_UnknownGame_Fails()
    {
        var result = new GameLibrary().PrepareLaunch("missing", new LaunchCommandBuilder());
        Assert.False(result.Success);
    }
}
