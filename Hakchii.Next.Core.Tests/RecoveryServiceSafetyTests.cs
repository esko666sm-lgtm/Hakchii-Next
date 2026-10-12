using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class RecoveryServiceSafetyTests
{
    [Fact]
    public void Constructor_RejectsNullBackupService()
    {
        Assert.Throws<ArgumentNullException>(() => new RecoveryService(null!));
    }

    [Fact]
    public async Task RestoreVerifiedAsync_NullVerifyResult_FailsCleanly()
    {
        var service = new RecoveryService(new Backup { NullVerify = true });

        var result = await service.RestoreVerifiedAsync(
            new BackupImage { Data = new byte[] { 1 }, Sha256 = "abc" },
            CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RestoreVerifiedAsync_VerifyException_FailsCleanlyWithoutRestore()
    {
        var backup = new Backup { ThrowVerify = true };
        var service = new RecoveryService(backup);

        var result = await service.RestoreVerifiedAsync(
            new BackupImage { Data = new byte[] { 1 }, Sha256 = "abc" },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, backup.RestoreCount);
    }

    [Fact]
    public async Task RestoreVerifiedAsync_RestoreException_FailsCleanly()
    {
        var service = new RecoveryService(new Backup { ThrowRestore = true });

        var result = await service.RestoreVerifiedAsync(
            new BackupImage { Data = new byte[] { 1 }, Sha256 = "abc" },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("restore", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class Backup : IBackupService
    {
        public bool NullVerify { get; set; }
        public bool ThrowVerify { get; set; }
        public bool ThrowRestore { get; set; }
        public int RestoreCount { get; private set; }

        public Task<BackupImage> CreateAsync(CancellationToken ct)
            => Task.FromResult(new BackupImage());

        public Task<OperationResult> VerifyAsync(BackupImage image, CancellationToken ct)
        {
            if (ThrowVerify) throw new InvalidOperationException("verify exploded");
            return Task.FromResult(NullVerify ? null! : OperationResult.Ok());
        }

        public Task<OperationResult> RestoreAsync(BackupImage image, CancellationToken ct)
        {
            RestoreCount++;
            if (ThrowRestore) throw new InvalidOperationException("restore exploded");
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
