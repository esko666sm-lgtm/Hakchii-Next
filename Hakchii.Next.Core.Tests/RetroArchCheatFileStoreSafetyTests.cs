using System;
using System.IO;
using Hakchii.Next.Core.Cheats;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class RetroArchCheatFileStoreSafetyTests
{
    [Fact]
    public void Save_RejectsMixedGameIdsWithoutCreatingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cht");
        try
        {
            var result = new RetroArchCheatFileStore().Save(path, new[]
            {
                new CheatEntry { Id = "1", GameId = "mario", Code = "AAAA" },
                new CheatEntry { Id = "2", GameId = "zelda", Code = "BBBB" }
            });

            Assert.False(result.Success);
            Assert.False(File.Exists(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Save_RejectsInvalidCheatWithoutReplacingExistingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cht");
        File.WriteAllText(path, "original");
        try
        {
            var result = new RetroArchCheatFileStore().Save(path, new[]
            {
                new CheatEntry { Id = "1", GameId = "mario", Code = "AAAA" },
                new CheatEntry { Id = "2", GameId = "mario", Code = "   " }
            });

            Assert.False(result.Success);
            Assert.Equal("original", File.ReadAllText(path));
            Assert.False(File.Exists(path + ".tmp"));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
        }
    }

    [Fact]
    public void Save_NormalizesEntriesBeforeSerialization()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cht");
        try
        {
            var cheat = new CheatEntry
            {
                Id = " 1 ",
                GameId = " mario ",
                Name = " Lives ",
                Code = " AAAA "
            };

            var result = new RetroArchCheatFileStore().Save(path, new[] { cheat });

            Assert.True(result.Success);
            Assert.Equal("1", cheat.Id);
            Assert.Equal("mario", cheat.GameId);
            Assert.Equal("Lives", cheat.Name);
            Assert.Equal("AAAA", cheat.Code);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
