using System;
using System.IO;
using System.Linq;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveFileStoreTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsSnapshot()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new SaveFileStore(dir);
            var snapshot = new SaveSnapshot { GameId = "mario", Data = new byte[] { 1, 2, 3 }, CreatedUtc = DateTime.UtcNow };

            var result = store.Save(snapshot);
            var loaded = store.LoadLatest("mario");

            Assert.True(result.Success);
            Assert.NotNull(loaded);
            Assert.Equal("mario", loaded.GameId);
            Assert.Equal(new byte[] { 1, 2, 3 }, loaded.Data);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadLatest_RejectsTamperedBackup()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new SaveFileStore(dir);
            Assert.True(store.Save(new SaveSnapshot { GameId = "mario", Data = new byte[] { 1, 2, 3 } }).Success);

            var dataFile = Directory.GetFiles(dir, "*.sav").Single();
            File.WriteAllBytes(dataFile, new byte[] { 9, 9, 9 });

            Assert.Null(store.LoadLatest("mario"));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void List_ReturnsNewestFirst()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new SaveFileStore(dir);
            Assert.True(store.Save(new SaveSnapshot { GameId = "mario", Data = new byte[] { 1 }, CreatedUtc = new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) }).Success);
            Assert.True(store.Save(new SaveSnapshot { GameId = "mario", Data = new byte[] { 2 }, CreatedUtc = new DateTime(2026,1,2,0,0,0,DateTimeKind.Utc) }).Success);

            var items = store.List("mario").ToArray();

            Assert.Equal(2, items.Length);
            Assert.True(items[0].CreatedUtc > items[1].CreatedUtc);
        }
        finally { Directory.Delete(dir, true); }
    }
}
