using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class FileBackupCreationSafetyTests
{
    [Fact]
    public async Task CreateAsync_RejectsSuccessfulTransportWithoutFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            var service = new VerifiedFileBackupService(new MissingFileTransport());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(path, CancellationToken.None));

            Assert.Contains("does not exist", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task CreateAsync_RejectsEmptyBackupFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            var service = new VerifiedFileBackupService(new EmptyFileTransport());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(path, CancellationToken.None));

            Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task VerifyAsync_RejectsEmptyBackupFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            var artifact = new BackupFileArtifact
            {
                Path = path,
                Sha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
            };

            var result = await new VerifiedFileBackupService(new MissingFileTransport())
                .VerifyAsync(artifact, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally { File.Delete(path); }
    }

    private sealed class MissingFileTransport : IBackupFileTransport
    {
        public Task<OperationResult> CreateAsync(string destinationPath, CancellationToken ct)
            => Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> RestoreAsync(string sourcePath, CancellationToken ct)
            => Task.FromResult(OperationResult.Ok());
    }

    private sealed class EmptyFileTransport : IBackupFileTransport
    {
        public Task<OperationResult> CreateAsync(string destinationPath, CancellationToken ct)
        {
            File.WriteAllBytes(destinationPath, Array.Empty<byte>());
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> RestoreAsync(string sourcePath, CancellationToken ct)
            => Task.FromResult(OperationResult.Ok());
    }
}
