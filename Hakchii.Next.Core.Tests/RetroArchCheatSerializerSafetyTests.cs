using System;
using Hakchii.Next.Core.Cheats;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class RetroArchCheatSerializerSafetyTests
{
    [Fact]
    public void Serialize_NullEntry_ThrowsClearArgumentException()
    {
        var serializer = new RetroArchCheatSerializer();

        var ex = Assert.Throws<ArgumentException>(() =>
            serializer.Serialize(new CheatEntry[] { null! }));

        Assert.Contains("cheat", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_NormalizesNameAndCode()
    {
        var serializer = new RetroArchCheatSerializer();
        var cheat = new CheatEntry
        {
            Id = "1",
            GameId = "mario",
            Name = "  Lives  ",
            Code = "  AAAA  "
        };

        var text = serializer.Serialize(new[] { cheat });

        Assert.Contains("cheat0_desc = \"Lives\"", text);
        Assert.Contains("cheat0_code = \"AAAA\"", text);
        Assert.Equal("Lives", cheat.Name);
        Assert.Equal("AAAA", cheat.Code);
    }
}
