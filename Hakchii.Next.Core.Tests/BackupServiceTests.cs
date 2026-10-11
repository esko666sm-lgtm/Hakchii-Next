using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class BackupServiceTests
{
    [Fact]
    public async Task CreateAsync_ComputesSha256()
    {
        var transport = new FakeBackupTransport(new byte[] { 1, 2, 3, 4 });
        var service = new VerifiedBackupService(transport);

        var image = await service.CreateAsync(CancellationToken.None);

        Assert.NotNull(image);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, image.Data);
        Assert.False(string.IsNullOrWhiteSpace(image.Sha256));
        Assert.NotEqual(default, image.CreatedUtc);
    }

    [Fact]
    public async Task VerifyAsync_RejectsModifiedImage()
    {
        var transport = new FakeBackupTransport(new byte[] { 1, 2, 3, 4 });
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);
        image.Data[0] = 9;

        var result = await service.VerifyAsync(image, CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(transport.RestoreCalled);
    }

    [Fact]
    public async Task RestoreAsync_RefusesUnverifiedImage()
    {
        var transport = new FakeBackupTransport(new byte[] { 1, 2, 3, 4 });
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);
        image.Sha256 = "bad";

        var result = await service.RestoreAsync(image, CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(transport.RestoreCalled);
    }

    [Fact]
    public async Task RestoreAsync_WritesVerifiedImage()
    {
        var transport = new FakeBackupTransport(new byte[] { 1, 2, 3, 4 });
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);

        var result = await service.RestoreAsync(image, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(transport.RestoreCalled);
        Assert.Equal(image.Data, transport.RestoredData);
    }

    private sealed class FakeBackupTransport : IBackupTransport
    {
        private readonly byte[] data;
        public bool RestoreCalled { get; private set; }
        public byte[] RestoredData { get; private set; } = Array.Empty<byte>();

        public FakeBackupTransport(byte[] data) { this.data = data; }

        public Task<byte[]> ReadAsync(CancellationToken ct)
            => Task.FromResult((byte[])data.Clone());

        public Task<OperationResult> WriteAsync(byte[] backupData, CancellationToken ct)
        {
            RestoreCalled = true;
            RestoredData = (byte[])backupData.Clone();
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
