using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class GameLibraryNormalizationTests
{
    [Fact]
    public void Add_TrimsIdAndRomPath()
    {
        var library = new GameLibrary();
        var game = new GameEntry
        {
            Id = "  mario  ",
            RomPath = "  /games/mario.nes  ",
            System = GameSystem.Nes
        };

        var result = library.Add(game);

        Assert.True(result.Success);
        Assert.Equal("mario", game.Id);
        Assert.Equal("/games/mario.nes", game.RomPath);
        Assert.Same(game, library.Find("mario"));
    }

    [Fact]
    public void Add_RejectsDuplicateAfterIdNormalization()
    {
        var library = new GameLibrary();
        Assert.True(library.Add(new GameEntry { Id = "mario", RomPath = "a.nes" }).Success);

        var result = library.Add(new GameEntry { Id = "  MARIO  ", RomPath = "b.nes" });

        Assert.False(result.Success);
        Assert.Single(library.Games);
    }

    [Fact]
    public void Update_UsesNormalizedId()
    {
        var library = new GameLibrary();
        Assert.True(library.Add(new GameEntry { Id = "mario", RomPath = "a.nes" }).Success);

        var updated = new GameEntry { Id = "  mario ", RomPath = "  b.nes " };
        var result = library.Update(updated);

        Assert.True(result.Success);
        Assert.Equal("mario", updated.Id);
        Assert.Equal("b.nes", library.Find("mario").RomPath);
    }
}
