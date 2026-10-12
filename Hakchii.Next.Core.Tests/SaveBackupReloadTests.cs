using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveBackupReloadTests
{
    [Fact]
    public void ReloadPersistentHistory_LoadsNewestSnapshotsIntoRam()
    {
        var provider = new Provider();
        var store = new Store(new[]
        {
            new SaveSnapshot { GameId = "mario", Data = new byte[] { 1 }, CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SaveSnapshot { GameId = "mario", Data = new byte[] { 2 }, CreatedUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
        });
        var service = new SaveBackupService(provider, maxBackupsPerGame: 10, persistentStore: store);

        var result = service.ReloadPersistentHistory("mario");

        Assert.True(result.Success);
        Assert.Equal(2, service.GetBackupCount("mario"));
    }

    [Fact]
    public async Task ReloadPersistentHistory_MakesLatestBackupRestorable()
    {
        var provider = new Provider();
        var store = new Store(new[]
        {
            new SaveSnapshot { GameId = "mario", Data = new byte[] { 1 }, CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SaveSnapshot { GameId = "mario", Data = new byte[] { 2 }, CreatedUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
        });
        var service = new SaveBackupService(provider, persistentStore: store);
        Assert.True(service.ReloadPersistentHistory("mario").Success);

        var result = await service.RestoreLatestAsync("mario", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(new byte[] { 2 }, provider.LastWritten);
    }

    [Fact]
    public void ReloadPersistentHistory_WithoutReadableStore_Fails()
    {
        var service = new SaveBackupService(new Provider(), persistentStore: new WriteOnlyStore());
        var result = service.ReloadPersistentHistory("mario");
        Assert.False(result.Success);
    }

    private sealed class Provider : ISaveProvider
    {
        public byte[] LastWritten { get; private set; } = Array.Empty<byte>();

        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(new SaveSnapshot { GameId = gameId, Data = new byte[] { 9 } });

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
        {
            LastWritten = (byte[])save.Data.Clone();
            return Task.FromResult(OperationResult.Ok());
        }
    }

    private sealed class Store : ISaveBackupStore, ISaveBackupHistoryStore
    {
        private readonly IReadOnlyCollection<SaveSnapshot> items;
        public Store(IReadOnlyCollection<SaveSnapshot> items) { this.items = items; }
        public OperationResult Save(SaveSnapshot snapshot) => OperationResult.Ok();
        public IReadOnlyCollection<SaveSnapshot> List(string gameId)
            => items.Where(x => x.GameId == gameId).OrderByDescending(x => x.CreatedUtc).ToArray();
    }

    private sealed class WriteOnlyStore : ISaveBackupStore
    {
        public OperationResult Save(SaveSnapshot snapshot) => OperationResult.Ok();
    }
}
