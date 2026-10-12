using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveRestoreSafetyTests
{
    [Fact]
    public async Task RestorePrevious_WriteException_KeepsBackupHistory()
    {
        var provider = new Provider { ThrowOnWrite = true };
        var service = new SaveBackupService(provider);
        Assert.True((await service.BackupAsync("mario", CancellationToken.None)).Success);

        var result = await service.RestorePreviousAsync("mario", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(1, service.GetBackupCount("mario"));
    }

    [Fact]
    public async Task RestoreLatest_NullProviderResult_FailsCleanly()
    {
        var provider = new Provider { ReturnNullOnWrite = true };
        var service = new SaveBackupService(provider);
        Assert.True((await service.BackupAsync("mario", CancellationToken.None)).Success);

        var result = await service.RestoreLatestAsync("mario", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(1, service.GetBackupCount("mario"));
    }

    private sealed class Provider : ISaveProvider
    {
        public bool ThrowOnWrite { get; set; }
        public bool ReturnNullOnWrite { get; set; }

        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(new SaveSnapshot { GameId = gameId, Data = new byte[] { 1, 2, 3 } });

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
        {
            if (ThrowOnWrite) throw new InvalidOperationException("restore exploded");
            return Task.FromResult(ReturnNullOnWrite ? null! : OperationResult.Ok());
        }
    }
}
