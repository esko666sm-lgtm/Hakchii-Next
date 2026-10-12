using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class DeviceStoragePathSafetyTests
{
    [Theory]
    [InlineData("", "/usr/share/games", "/rootfs")]
    [InlineData("relative/games", "/usr/share/games", "/rootfs")]
    [InlineData("/var/lib/hakchi/games", "", "/rootfs")]
    [InlineData("/var/lib/hakchi/games", "games", "/rootfs")]
    [InlineData("/var/lib/hakchi/games", "/usr/share/games", "rootfs")]
    public async Task ReadDetails_RejectsInvalidDevicePaths(string storage, string original, string rootfs)
    {
        var transport = new FakeTransport(storage, original, rootfs);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new DeviceInfoService(transport).ReadDetailsAsync("ABC123", CancellationToken.None));
    }

    [Fact]
    public async Task ReadDetails_AcceptsAbsoluteUnixPaths()
    {
        var transport = new FakeTransport("/var/lib/hakchi/games", "/usr/share/games", "/rootfs");
        var info = await new DeviceInfoService(transport).ReadDetailsAsync("ABC123", CancellationToken.None);

        Assert.Equal("/var/lib/hakchi/games", info.GameStoragePath);
        Assert.Equal("/usr/share/games", info.OriginalGamesPath);
        Assert.Equal("/rootfs", info.RootFsPath);
    }

    private sealed class FakeTransport : IDeviceCommandTransport
    {
        private readonly string storage;
        private readonly string original;
        private readonly string rootfs;

        public FakeTransport(string storage, string original, string rootfs)
        {
            this.storage = storage;
            this.original = original;
            this.rootfs = rootfs;
        }

        public bool IsOnline => true;
        public Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string> ExecuteAsync(string command, CancellationToken ct)
        {
            if (command.Contains("sftype")) return Task.FromResult("nes-usa");
            if (command.Contains("findGameSyncStorage")) return Task.FromResult(storage);
            if (command.Contains("gamepath")) return Task.FromResult(original);
            if (command.Contains("rootfs")) return Task.FromResult(rootfs);
            return Task.FromResult("ABC123");
        }
    }
}
