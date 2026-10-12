using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class DeviceIdentitySafetyTests
{
    [Fact]
    public async Task ReadHardwareId_RemovesAllWhitespace()
    {
        var transport = new Transport { HardwareId = " AB\tC 12\r\n3 " };
        var id = await new DeviceInfoService(transport).ReadHardwareIdAsync(CancellationToken.None);
        Assert.Equal("ABC123", id);
    }

    [Fact]
    public async Task ReadAsync_RejectsEmptyHardwareId()
    {
        var transport = new Transport { HardwareId = " \r\n\t " };
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new DeviceInfoService(transport).ReadAsync(CancellationToken.None));
        Assert.Single(transport.Commands);
    }

    [Fact]
    public async Task ReadDetails_RejectsEmptySystemCode()
    {
        var transport = new Transport { SystemCode = "  " };
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new DeviceInfoService(transport).ReadDetailsAsync("ABC123", CancellationToken.None));
    }

    private sealed class Transport : IDeviceCommandTransport
    {
        public string HardwareId { get; set; } = "ABC123";
        public string SystemCode { get; set; } = "nes-usa";
        public bool IsOnline => true;
        public List<string> Commands { get; } = new List<string>();

        public Task ConnectAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

        public Task<string> ExecuteAsync(string command, CancellationToken ct)
        {
            Commands.Add(command);
            if (command == "hakchi hwid") return Task.FromResult(HardwareId);
            if (command.Contains("sftype")) return Task.FromResult(SystemCode);
            if (command.Contains("findGameSyncStorage")) return Task.FromResult("/var/lib/hakchi/games");
            if (command.Contains("gamepath")) return Task.FromResult("/usr/share/games");
            return Task.FromResult("/rootfs");
        }
    }
}
