using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class DeviceProbeSafetyTests
{
    [Fact]
    public void DeviceInfoService_RejectsNullTransport()
    {
        Assert.Throws<ArgumentNullException>(() => new DeviceInfoService(null!));
    }

    [Fact]
    public async Task ProbeAsync_HardwareTransportException_ReturnsNotFound()
    {
        var probe = new TransportDeviceProbe(new Transport { ThrowOn = "hakchi hwid" });

        var result = await probe.ProbeAsync(CancellationToken.None);

        Assert.False(result.Found);
    }

    [Fact]
    public async Task ProbeAsync_SystemCodeTransportException_ReturnsNotFound()
    {
        var probe = new TransportDeviceProbe(new Transport { ThrowOn = "hakchi eval" });

        var result = await probe.ProbeAsync(CancellationToken.None);

        Assert.False(result.Found);
    }

    private sealed class Transport : IDeviceCommandTransport
    {
        public bool IsOnline => true;
        public string ThrowOn { get; set; } = string.Empty;

        public Task ConnectAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

        public Task<string> ExecuteAsync(string command, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(ThrowOn) && command.Contains(ThrowOn, StringComparison.Ordinal))
                throw new IOException("transport failed");

            if (command == "hakchi hwid")
                return Task.FromResult("ABC123");

            return Task.FromResult("nes-usa");
        }
    }
}
