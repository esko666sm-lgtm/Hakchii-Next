using Hakchii.Next.Core.Cheats;
using Hakchii.Next.Core.Games;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CheatImportAtomicityTests
{
    [Fact]
    public void ImportRetroArch_DuplicateMidBatch_DoesNotPartiallyCommit()
    {
        var db = new CheatDatabase();
        Assert.True(db.Add(new CheatEntry
        {
            Id = "mario:1",
            GameId = "mario",
            Code = "OLD"
        }).Success);

        var text =
            "cheats = \"2\"\n" +
            "cheat0_desc = \"Lives\"\n" +
            "cheat0_code = \"AAAA\"\n" +
            "cheat1_desc = \"Duplicate\"\n" +
            "cheat1_code = \"BBBB\"\n";

        var result = db.ImportRetroArch("mario", text);

        Assert.False(result.Success);
        var items = db.Find(new GameEntry { Id = "mario" });
        Assert.Single(items);
        Assert.Contains(items, c => c.Code == "OLD");
        Assert.DoesNotContain(items, c => c.Code == "AAAA");
    }

    [Fact]
    public void ImportRetroArch_ValidBatch_CommitsAll()
    {
        var db = new CheatDatabase();
        var text =
            "cheats = \"2\"\n" +
            "cheat0_desc = \"Lives\"\n" +
            "cheat0_code = \"AAAA\"\n" +
            "cheat1_desc = \"Invincible\"\n" +
            "cheat1_code = \"BBBB\"\n";

        var result = db.ImportRetroArch("mario", text);

        Assert.True(result.Success);
        Assert.Equal(2, db.Find(new GameEntry { Id = "mario" }).Count);
    }
}
