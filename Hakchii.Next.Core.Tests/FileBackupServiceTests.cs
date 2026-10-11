using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class FileBackupServiceTests
{
    [Fact]
    public async Task CreateAsync_HashesBackupFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            var transport = new FakeFileBackupTransport(new byte[] { 1, 2, 3, 4 });
            var service = new VerifiedFileBackupService(transport);

            var artifact = await service.CreateAsync(path, CancellationToken.None);

            Assert.Equal(path, artifact.Path);
            Assert.False(string.IsNullOrWhiteSpace(artifact.Sha256));
            Assert.True(File.Exists(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RestoreVerifiedAsync_RejectsModifiedFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            var transport = new FakeFileBackupTransport(new byte[] { 1, 2, 3, 4 });
            var service = new VerifiedFileBackupService(transport);
            var artifact = await service.CreateAsync(path, CancellationToken.None);
            File.WriteAllBytes(path, new byte[] { 9, 2, 3, 4 });

            var result = await service.RestoreVerifiedAsync(artifact, CancellationToken.None);

            Assert.False(result.Success);
            Assert.False(transport.RestoreCalled);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RestoreVerifiedAsync_PassesVerifiedPathToTransport()
    {
        var path = Path.GetTempFileName();
        try
        {
            var transport = new FakeFileBackupTransport(new byte[] { 1, 2, 3, 4 });
            var service = new VerifiedFileBackupService(transport);
            var artifact = await service.CreateAsync(path, CancellationToken.None);

            var result = await service.RestoreVerifiedAsync(artifact, CancellationToken.None);

            Assert.True(result.Success);
            Assert.True(transport.RestoreCalled);
            Assert.Equal(path, transport.RestoredPath);
        }
        finally { File.Delete(path); }
    }

    private sealed class FakeFileBackupTransport : IBackupFileTransport
    {
        private readonly byte[] data;
        public bool RestoreCalled { get; private set; }
        public string RestoredPath { get; private set; } = "";

        public FakeFileBackupTransport(byte[] data) { this.data = data; }

        public Task<OperationResult> CreateAsync(string destinationPath, CancellationToken ct)
        {
            File.WriteAllBytes(destinationPath, data);
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> RestoreAsync(string sourcePath, CancellationToken ct)
        {
            RestoreCalled = true;
            RestoredPath = sourcePath;
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
