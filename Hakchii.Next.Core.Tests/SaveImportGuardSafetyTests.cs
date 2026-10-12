using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class SaveImportGuardSafetyTests
{
    [Fact]
    public async Task Replace_BackupFailure_PreventsReplacementWrite()
    {
        var provider = new Provider
        {
            Current = new SaveSnapshot { GameId = "mario", Data = Array.Empty<byte>() }
        };
        var guard = new SaveImportGuard(new SaveBackupService(provider), provider);

        var result = await guard.ReplaceAsync(
            "mario",
            new SaveSnapshot { GameId = "mario", Data = new byte[] { 9 } },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, provider.WriteCount);
    }

    [Fact]
    public async Task Replace_WriteException_RollsBackLatestBackup()
    {
        var provider = new Provider
        {
            Current = new SaveSnapshot { GameId = "mario", Data = new byte[] { 1, 2, 3 } },
            ThrowOnFirstWrite = true
        };
        var backups = new SaveBackupService(provider);
        var guard = new SaveImportGuard(backups, provider);

        var result = await guard.ReplaceAsync(
            "mario",
            new SaveSnapshot { GameId = "mario", Data = new byte[] { 9 } },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(new byte[] { 1, 2, 3 }, provider.Current.Data);
        Assert.Equal(2, provider.WriteCount);
    }

    private sealed class Provider : ISaveProvider
    {
        public SaveSnapshot Current { get; set; } = new SaveSnapshot
        {
            GameId = "mario",
            Data = new byte[] { 1 }
        };

        public bool ThrowOnFirstWrite { get; set; }
        public int WriteCount { get; private set; }

        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct)
            => Task.FromResult(Current);

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct)
        {
            WriteCount++;
            if (ThrowOnFirstWrite && WriteCount == 1)
                throw new InvalidOperationException("write exploded");

            Current = new SaveSnapshot
            {
                GameId = save.GameId,
                Data = (byte[])save.Data.Clone(),
                CreatedUtc = save.CreatedUtc
            };
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
