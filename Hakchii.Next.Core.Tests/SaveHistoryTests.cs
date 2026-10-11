using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveHistoryTests
{
    [Fact]
    public async Task BackupHistory_IsBounded()
    {
        var provider = new FakeProvider();
        var service = new SaveBackupService(provider, maxBackupsPerGame: 2);

        provider.Data = new byte[] { 1 };
        Assert.True((await service.BackupAsync("mario", CancellationToken.None)).Success);
        provider.Data = new byte[] { 2 };
        Assert.True((await service.BackupAsync("mario", CancellationToken.None)).Success);
        provider.Data = new byte[] { 3 };
        Assert.True((await service.BackupAsync("mario", CancellationToken.None)).Success);

        Assert.Equal(2, service.GetBackupCount("mario"));
    }

    [Fact]
    public async Task RestorePrevious_PopsLatestBackup()
    {
        var provider = new FakeProvider();
        var service = new SaveBackupService(provider);

        provider.Data = new byte[] { 1 };
        await service.BackupAsync("mario", CancellationToken.None);
        provider.Data = new byte[] { 2 };
        await service.BackupAsync("mario", CancellationToken.None);

        provider.Data = new byte[] { 9 };
        var result = await service.RestorePreviousAsync("mario", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(new byte[] { 2 }, provider.Data);
        Assert.Equal(1, service.GetBackupCount("mario"));
    }

    private sealed class FakeProvider : ISaveProvider
    {
        public byte[] Data { get; set; } = new byte[] { 1 };

        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(new SaveSnapshot { GameId = gameId, Data = (byte[])Data.Clone() });

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
        {
            Data = (byte[])save.Data.Clone();
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
