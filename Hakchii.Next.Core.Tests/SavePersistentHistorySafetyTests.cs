using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SavePersistentHistorySafetyTests
{
    [Fact]
    public void ReloadPersistentHistory_StoreException_FailsCleanly()
    {
        var store = new HistoryStore { ThrowOnList = true };
        var service = new SaveBackupService(new Provider(), persistentStore: store);

        var result = service.ReloadPersistentHistory("mario");

        Assert.False(result.Success);
        Assert.Equal(0, service.GetBackupCount("mario"));
    }

    [Fact]
    public void ReloadPersistentHistory_IgnoresInvalidSnapshotsAndKeepsValidOnes()
    {
        var store = new HistoryStore
        {
            Items = new SaveSnapshot[]
            {
                null!,
                new SaveSnapshot { GameId = "zelda", Data = new byte[] { 1 } },
                new SaveSnapshot { GameId = "mario", Data = Array.Empty<byte>() },
                new SaveSnapshot { GameId = "mario", Data = new byte[] { 9 }, CreatedUtc = new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) }
            }
        };
        var service = new SaveBackupService(new Provider(), persistentStore: store);

        var result = service.ReloadPersistentHistory("mario");

        Assert.True(result.Success);
        Assert.Equal(1, service.GetBackupCount("mario"));
    }

    private sealed class Provider : ISaveProvider
    {
        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(new SaveSnapshot { GameId = gameId, Data = new byte[] { 1 } });

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
            => Task.FromResult(OperationResult.Ok());
    }

    private sealed class HistoryStore : ISaveBackupStore, ISaveBackupHistoryStore
    {
        public bool ThrowOnList { get; set; }
        public IReadOnlyCollection<SaveSnapshot> Items { get; set; } = Array.Empty<SaveSnapshot>();

        public OperationResult Save(SaveSnapshot snapshot) => OperationResult.Ok();

        public IReadOnlyCollection<SaveSnapshot> List(string gameId)
        {
            if (ThrowOnList) throw new InvalidOperationException("history failed");
            return Items;
        }
    }
}
