using System;
using System.IO;
using System.Linq;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveFileStoreSafetyTests
{
    [Fact]
    public void List_DuplicateMetadataKey_SkipsCorruptEntry()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new SaveFileStore(dir);
            Assert.True(store.Save(new SaveSnapshot
            {
                GameId = "mario",
                Data = new byte[] { 1, 2, 3 },
                CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }).Success);

            var meta = Directory.GetFiles(dir, "*.meta").Single();
            File.AppendAllText(meta, Environment.NewLine + "size=3");

            var items = store.List("mario");

            Assert.Empty(items);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void SaveAndList_NormalizeGameId()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new SaveFileStore(dir);
            var snapshot = new SaveSnapshot
            {
                GameId = "  mario  ",
                Data = new byte[] { 9 }
            };

            var result = store.Save(snapshot);
            var loaded = store.LoadLatest("mario");

            Assert.True(result.Success);
            Assert.Equal("mario", snapshot.GameId);
            Assert.NotNull(loaded);
            Assert.Equal("mario", loaded.GameId);
        }
        finally { Directory.Delete(dir, true); }
    }
}
