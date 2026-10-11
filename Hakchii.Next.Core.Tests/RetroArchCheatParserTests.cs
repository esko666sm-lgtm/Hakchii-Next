using System.Linq;
using Hakchii.Next.Core.Cheats;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class RetroArchCheatParserTests
{
    [Fact]
    public void Parse_ReadsMultipleCheats()
    {
        var text = "cheats = \"2\"\n" +
                   "cheat0_desc = \"Infinite Lives\"\n" +
                   "cheat0_code = \"AAAA-AAAA\"\n" +
                   "cheat0_enable = \"true\"\n" +
                   "cheat1_desc = \"Invincible\"\n" +
                   "cheat1_code = \"BBBB-BBBB\"\n" +
                   "cheat1_enable = \"false\"\n";

        var cheats = new RetroArchCheatParser().Parse(text, "mario").ToArray();

        Assert.Equal(2, cheats.Length);
        Assert.Equal("mario", cheats[0].GameId);
        Assert.Equal("Infinite Lives", cheats[0].Name);
        Assert.Equal("AAAA-AAAA", cheats[0].Code);
        Assert.True(cheats[0].Enabled);
        Assert.False(cheats[1].Enabled);
    }

    [Fact]
    public void Parse_UnescapesDescriptionAndCode()
    {
        var text = "cheats = \"1\"\n" +
                   "cheat0_desc = \"Quote \\\"test\\\"\"\n" +
                   "cheat0_code = \"AA\\\\BB\"\n";

        var cheat = Assert.Single(new RetroArchCheatParser().Parse(text, "game"));

        Assert.Equal("Quote \"test\"", cheat.Name);
        Assert.Equal("AA\\BB", cheat.Code);
    }

    [Fact]
    public void Parse_MissingCode_SkipsEntry()
    {
        var text = "cheats = \"1\"\ncheat0_desc = \"Broken\"\n";
        Assert.Empty(new RetroArchCheatParser().Parse(text, "game"));
    }

    [Fact]
    public void Import_AddsParsedEntriesToDatabase()
    {
        var db = new CheatDatabase();
        var text = "cheats = \"1\"\ncheat0_desc = \"Lives\"\ncheat0_code = \"AAAA-AAAA\"\n";

        var result = db.ImportRetroArch("mario", text);

        Assert.True(result.Success);
        Assert.Single(db.Find(new Hakchii.Next.Core.Games.GameEntry { Id = "mario" }));
    }
}
