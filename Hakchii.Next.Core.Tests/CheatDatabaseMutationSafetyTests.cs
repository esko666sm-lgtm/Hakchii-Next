using Hakchii.Next.Core.Cheats;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CheatDatabaseMutationSafetyTests
{
    [Fact]
    public void Add_StoresIndependentSnapshot()
    {
        var db = new CheatDatabase();
        var cheat = new CheatEntry
        {
            Id = "lives",
            GameId = "mario",
            Name = "Lives",
            Code = "AAAA",
            Enabled = true
        };

        Assert.True(db.Add(cheat).Success);

        cheat.Id = "mutated";
        cheat.GameId = "zelda";
        cheat.Name = "Changed";
        cheat.Code = "BBBB";
        cheat.Enabled = false;

        var stored = Assert.Single(db.Find(new GameEntry { Id = "mario" }));
        Assert.Equal("lives", stored.Id);
        Assert.Equal("mario", stored.GameId);
        Assert.Equal("Lives", stored.Name);
        Assert.Equal("AAAA", stored.Code);
        Assert.True(stored.Enabled);
        Assert.Empty(db.Find(new GameEntry { Id = "zelda" }));
    }
}
