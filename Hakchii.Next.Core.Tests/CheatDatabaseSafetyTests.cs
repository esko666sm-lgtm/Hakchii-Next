using Hakchii.Next.Core.Cheats;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CheatDatabaseSafetyTests
{
    [Fact]
    public void Add_NormalizesIdentifiersAndCode()
    {
        var db = new CheatDatabase();
        var cheat = new CheatEntry
        {
            Id = "  lives  ",
            GameId = "  mario  ",
            Name = " Lives ",
            Code = "  AAAA-AAAA  "
        };

        var result = db.Add(cheat);

        Assert.True(result.Success);
        Assert.Equal("lives", cheat.Id);
        Assert.Equal("mario", cheat.GameId);
        Assert.Equal("AAAA-AAAA", cheat.Code);
        Assert.Single(db.Find(new GameEntry { Id = "MARIO" }));
    }

    [Fact]
    public void Add_AllowsSameCheatIdForDifferentGames()
    {
        var db = new CheatDatabase();

        Assert.True(db.Add(new CheatEntry { Id = "1", GameId = "mario", Code = "AAAA" }).Success);
        var result = db.Add(new CheatEntry { Id = "1", GameId = "zelda", Code = "BBBB" });

        Assert.True(result.Success);
        Assert.Single(db.Find(new GameEntry { Id = "mario" }));
        Assert.Single(db.Find(new GameEntry { Id = "zelda" }));
    }

    [Fact]
    public void Add_RejectsDuplicateIdWithinSameGameAfterNormalization()
    {
        var db = new CheatDatabase();
        Assert.True(db.Add(new CheatEntry { Id = "1", GameId = "mario", Code = "AAAA" }).Success);

        var result = db.Add(new CheatEntry { Id = " 1 ", GameId = " MARIO ", Code = "BBBB" });

        Assert.False(result.Success);
        Assert.Single(db.Find(new GameEntry { Id = "mario" }));
    }
}
