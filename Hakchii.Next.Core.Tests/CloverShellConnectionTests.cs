using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CloverShellConnectionTests
{
    [Fact]
    public async Task Connect_Succeeds_WhenSupportedDeviceAndTransportOnline()
    {
        var transport = new Transport();
        var connection = new CloverShellConsoleConnection(transport, Detector(true));
        var result = await connection.ConnectAsync(CancellationToken.None);
        Assert.True(result.Success);
        Assert.True(connection.IsConnected);
        Assert.Equal(ConsoleFamily.NesMini, connection.Profile.Family);
    }

    [Fact]
    public async Task Connect_FailsCleanly_WhenNoDeviceFound()
    {
        var transport = new Transport();
        var connection = new CloverShellConsoleConnection(transport, Detector(false));
        var result = await connection.ConnectAsync(CancellationToken.None);
        Assert.False(result.Success);
        Assert.False(transport.ConnectCalled);
    }

    private static IDeviceDetector Detector(bool found) => new DeviceDetector(new Probe(found));

    private sealed class Probe : IDeviceProbe
    {
        private readonly bool found;
        public Probe(bool found) { this.found = found; }
        public Task<DeviceProbeResult> ProbeAsync(CancellationToken cancellationToken) => Task.FromResult(new DeviceProbeResult { Found = found, HardwareId = "USB\\VID_1F3A&PID_EFE8", ProductName = "Nintendo Classic Mini: NES" });
    }

    private sealed class Transport : ICloverShellTransport
    {
        public bool IsOnline { get; private set; }
        public bool ConnectCalled { get; private set; }
        public Task ConnectAsync(CancellationToken cancellationToken) { ConnectCalled = true; IsOnline = true; return Task.CompletedTask; }
        public Task DisconnectAsync(CancellationToken cancellationToken) { IsOnline = false; return Task.CompletedTask; }
    }
}
