using Hakchii.Next.Core.Cheats;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class RetroArchCheatParserSafetyTests
{
    [Fact]
    public void Parse_DuplicateKey_RejectsWholeFile()
    {
        var text =
            "cheats = \"1\"\n" +
            "cheat0_desc = \"Lives\"\n" +
            "cheat0_code = \"AAAA\"\n" +
            "cheat0_code = \"BBBB\"\n";

        var items = new RetroArchCheatParser().Parse(text, "mario");

        Assert.Empty(items);
    }

    [Fact]
    public void Parse_NormalizesGameIdAndValues()
    {
        var text =
            "cheats = \"1\"\n" +
            "cheat0_desc = \"  Lives  \"\n" +
            "cheat0_code = \"  AAAA  \"\n";

        var items = new RetroArchCheatParser().Parse(text, "  mario  ");

        var cheat = Assert.Single(items);
        Assert.Equal("mario", cheat.GameId);
        Assert.Equal("mario:0", cheat.Id);
        Assert.Equal("Lives", cheat.Name);
        Assert.Equal("AAAA", cheat.Code);
    }
}
