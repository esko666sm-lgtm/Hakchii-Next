using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class VerifiedFileBackupRestoreSafetyTests
{
    [Fact]
    public async Task VerifyAsync_LockedFile_FailsCleanly()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            var service = new VerifiedFileBackupService(new Transport());
            var artifact = await service.CreateAsync(path, CancellationToken.None);

            using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var result = await service.VerifyAsync(artifact, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Contains("verify", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task RestoreVerifiedAsync_NullTransportResult_FailsCleanly()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            var transport = new Transport { ReturnNullOnRestore = true };
            var service = new VerifiedFileBackupService(transport);
            var artifact = await service.CreateAsync(path, CancellationToken.None);

            var result = await service.RestoreVerifiedAsync(artifact, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Contains("restore", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private sealed class Transport : IBackupFileTransport
    {
        public bool ReturnNullOnRestore { get; set; }

        public Task<OperationResult> CreateAsync(string destinationPath, CancellationToken ct)
        {
            if (!File.Exists(destinationPath))
                File.WriteAllBytes(destinationPath, new byte[] { 1, 2, 3 });
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> RestoreAsync(string sourcePath, CancellationToken ct)
            => Task.FromResult(ReturnNullOnRestore ? null! : OperationResult.Ok());
    }
}
