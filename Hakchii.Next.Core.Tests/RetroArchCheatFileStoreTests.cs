using System;
using System.IO;
using System.Linq;
using Hakchii.Next.Core.Cheats;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class RetroArchCheatFileStoreTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsCheats()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-cheat-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "mario.cht");
        try
        {
            var store = new RetroArchCheatFileStore();
            var cheats = new[]
            {
                new CheatEntry { Id = "1", GameId = "mario", Name = "Lives", Code = "AAAA-AAAA", Enabled = true },
                new CheatEntry { Id = "2", GameId = "mario", Name = "Invincible", Code = "BBBB-BBBB", Enabled = false }
            };

            var save = store.Save(path, cheats);
            var loaded = store.Load(path, "mario").ToArray();

            Assert.True(save.Success);
            Assert.Equal(2, loaded.Length);
            Assert.Equal("Lives", loaded[0].Name);
            Assert.True(loaded[0].Enabled);
            Assert.False(loaded[1].Enabled);
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Save_ReplacesExistingFileAtomically()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-cheat-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "mario.cht");
        try
        {
            File.WriteAllText(path, "old");
            var store = new RetroArchCheatFileStore();
            var result = store.Save(path, new[]
            {
                new CheatEntry { Id = "1", GameId = "mario", Name = "Lives", Code = "AAAA-AAAA", Enabled = true }
            });

            Assert.True(result.Success);
            Assert.Contains("cheat0_code", File.ReadAllText(path));
            Assert.False(File.Exists(path + ".tmp"));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Load_MissingFile_ReturnsEmpty()
    {
        var store = new RetroArchCheatFileStore();
        Assert.Empty(store.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cht"), "mario"));
    }

    [Fact]
    public void ImportIntoDatabase_LoadsCheatsFromFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-cheat-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "mario.cht");
        try
        {
            File.WriteAllText(path, "cheats = \"1\"\ncheat0_desc = \"Lives\"\ncheat0_code = \"AAAA-AAAA\"\n");
            var db = new CheatDatabase();
            var result = new RetroArchCheatFileStore().ImportInto(db, path, "mario");

            Assert.True(result.Success);
            Assert.Single(db.Find(new Hakchii.Next.Core.Games.GameEntry { Id = "mario" }));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}
