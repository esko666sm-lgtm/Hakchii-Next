using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveSafetyTests
{
    [Fact]
    public async Task Backup_RejectsEmptySaveData()
    {
        var provider = new FakeProvider(new SaveSnapshot { GameId = "mario", Data = null });
        var result = await new SaveBackupService(provider).BackupAsync("mario", CancellationToken.None);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Replace_RejectsSaveForDifferentGame()
    {
        var provider = new FakeProvider(new SaveSnapshot { GameId = "mario", Data = new byte[] { 1, 2, 3 } });
        var backups = new SaveBackupService(provider);
        var guard = new SaveImportGuard(backups, provider);

        var result = await guard.ReplaceAsync(
            "mario",
            new SaveSnapshot { GameId = "zelda", Data = new byte[] { 9 } },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, provider.WriteCount);
    }

    [Fact]
    public async Task Replace_RejectsEmptyReplacementData()
    {
        var provider = new FakeProvider(new SaveSnapshot { GameId = "mario", Data = new byte[] { 1, 2, 3 } });
        var backups = new SaveBackupService(provider);
        var guard = new SaveImportGuard(backups, provider);

        var result = await guard.ReplaceAsync(
            "mario",
            new SaveSnapshot { GameId = "mario", Data = null },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, provider.WriteCount);
    }

    private sealed class FakeProvider : ISaveProvider
    {
        private SaveSnapshot current;
        public int WriteCount { get; private set; }

        public FakeProvider(SaveSnapshot current) { this.current = current; }

        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(current);

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
        {
            WriteCount++;
            current = save;
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
