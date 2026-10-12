using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class BackupImageSafetyTests
{
    [Fact]
    public async Task CreateAsync_ClonesTransportBuffer()
    {
        var source = new byte[] { 1, 2, 3 };
        var transport = new Transport(source);
        var service = new VerifiedBackupService(transport);

        var image = await service.CreateAsync(CancellationToken.None);
        source[0] = 9;

        Assert.Equal(new byte[] { 1, 2, 3 }, image.Data);
    }

    [Fact]
    public async Task RestoreAsync_PassesCloneToTransport()
    {
        var transport = new Transport(new byte[] { 1, 2, 3 });
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);

        var result = await service.RestoreAsync(image, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotSame(image.Data, transport.RestoredData);
        Assert.Equal(image.Data, transport.RestoredData);
    }

    [Fact]
    public async Task RestoreAsync_TransportReturningNullResult_FailsCleanly()
    {
        var transport = new Transport(new byte[] { 1, 2, 3 }) { ReturnNullOnWrite = true };
        var service = new VerifiedBackupService(transport);
        var image = await service.CreateAsync(CancellationToken.None);

        var result = await service.RestoreAsync(image, CancellationToken.None);

        Assert.False(result.Success);
    }

    private sealed class Transport : IBackupTransport
    {
        private readonly byte[] data;
        public bool ReturnNullOnWrite { get; set; }
        public byte[] RestoredData { get; private set; } = Array.Empty<byte>();

        public Transport(byte[] data) { this.data = data; }

        public Task<byte[]> ReadAsync(CancellationToken ct)
            => Task.FromResult(data);

        public Task<OperationResult> WriteAsync(byte[] backupData, CancellationToken ct)
        {
            RestoredData = backupData;
            return Task.FromResult(ReturnNullOnWrite ? null! : OperationResult.Ok());
        }
    }
}
