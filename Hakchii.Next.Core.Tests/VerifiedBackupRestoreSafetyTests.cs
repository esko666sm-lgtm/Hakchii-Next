using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class VerifiedBackupRestoreSafetyTests
{
    [Fact]
    public async Task RestoreAsync_WriteException_FailsCleanly()
    {
        var transport = new Transport { ThrowOnWrite = true };
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);

        var result = await service.RestoreAsync(image, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("restore", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RestoreAsync_CancellationStillPropagates()
    {
        var transport = new Transport { CancelOnWrite = true };
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.RestoreAsync(image, CancellationToken.None));
    }

    private sealed class Transport : IBackupTransport
    {
        public bool ThrowOnWrite { get; set; }
        public bool CancelOnWrite { get; set; }

        public Task<byte[]> ReadAsync(CancellationToken ct)
            => Task.FromResult(new byte[] { 1, 2, 3, 4 });

        public Task<OperationResult> WriteAsync(byte[] backupData, CancellationToken ct)
        {
            if (CancelOnWrite) throw new OperationCanceledException();
            if (ThrowOnWrite) throw new InvalidOperationException("write exploded");
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
