using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveBackupPersistenceTests
{
    [Fact]
    public async Task Backup_WithPersistentStore_MirrorsSnapshot()
    {
        var provider = new Provider();
        var store = new Store();
        var service = new SaveBackupService(provider, persistentStore: store);

        var result = await service.BackupAsync("mario", CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(store.Last);
        Assert.Equal("mario", store.Last.GameId);
        Assert.Equal(new byte[] { 1, 2, 3 }, store.Last.Data);
        Assert.Equal(1, service.GetBackupCount("mario"));
    }

    [Fact]
    public async Task Backup_PersistentStoreFailure_DoesNotCommitRamBackup()
    {
        var provider = new Provider();
        var store = new Store { Fail = true };
        var service = new SaveBackupService(provider, persistentStore: store);

        var result = await service.BackupAsync("mario", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, service.GetBackupCount("mario"));
    }

    private sealed class Provider : ISaveProvider
    {
        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(new SaveSnapshot { GameId = gameId, Data = new byte[] { 1, 2, 3 } });

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
            => Task.FromResult(OperationResult.Ok());
    }

    private sealed class Store : ISaveBackupStore
    {
        public bool Fail { get; set; }
        public SaveSnapshot? Last { get; private set; }

        public OperationResult Save(SaveSnapshot snapshot)
        {
            Last = snapshot;
            return Fail ? OperationResult.Fail("disk failed") : OperationResult.Ok();
        }
    }
}
