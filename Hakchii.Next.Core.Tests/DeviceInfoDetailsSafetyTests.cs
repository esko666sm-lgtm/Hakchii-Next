using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class DeviceInfoDetailsSafetyTests
{
    [Fact]
    public async Task ReadDetailsAsync_NormalizesHardwareId()
    {
        var service = new DeviceInfoService(new Transport());

        var info = await service.ReadDetailsAsync("  ABC123  ", CancellationToken.None);

        Assert.Equal("ABC123", info.HardwareId);
    }

    [Fact]
    public async Task ReadDetailsAsync_NullSystemCode_FailsCleanly()
    {
        var service = new DeviceInfoService(new Transport { NullOnSystemCode = true });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReadDetailsAsync("ABC123", CancellationToken.None));

        Assert.Contains("system code", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadDetailsAsync_NullGameStoragePath_FailsCleanly()
    {
        var service = new DeviceInfoService(new Transport { NullOnGameStorage = true });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReadDetailsAsync("ABC123", CancellationToken.None));

        Assert.Contains("storage", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class Transport : IDeviceCommandTransport
    {
        public bool IsOnline => true;
        public bool NullOnSystemCode { get; set; }
        public bool NullOnGameStorage { get; set; }

        public Task ConnectAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

        public Task<string> ExecuteAsync(string command, CancellationToken ct)
        {
            if (command.Contains("$sftype-$sfregion", StringComparison.Ordinal))
                return Task.FromResult(NullOnSystemCode ? null! : "nes-usa");
            if (command == "hakchi findGameSyncStorage")
                return Task.FromResult(NullOnGameStorage ? null! : "/var/lib/hakchi/games");
            if (command == "hakchi get gamepath")
                return Task.FromResult("/usr/share/games");
            if (command == "hakchi get rootfs")
                return Task.FromResult("/");
            if (command == "hakchi hwid")
                return Task.FromResult("ABC123");
            return Task.FromResult(string.Empty);
        }
    }
}
