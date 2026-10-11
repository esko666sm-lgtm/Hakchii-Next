using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class DeviceDetectorTests
{
    [Fact]
    public async Task Detects_NesMini_FromKnownCloverUsbId()
    {
        var detector = new DeviceDetector(new Probe(new DeviceProbeResult { Found = true, HardwareId = "USB\\VID_1F3A&PID_EFE8", ProductName = "Nintendo Classic Mini: NES" }));
        var profile = await detector.DetectAsync(CancellationToken.None);
        Assert.Equal(ConsoleFamily.NesMini, profile.Family);
        Assert.True(profile.Capabilities.SupportsCloverShell);
    }

    [Fact]
    public async Task NoDevice_ReturnsNull()
    {
        var detector = new DeviceDetector(new Probe(new DeviceProbeResult { Found = false }));
        Assert.Null(await detector.DetectAsync(CancellationToken.None));
    }

    [Fact]
    public async Task TransportProbe_UsesLiveHakchiFacts()
    {
        var transport = new FakeTransport(true, new Dictionary<string, string>
        {
            ["hakchi hwid"] = "AA BB CC",
            ["hakchi eval 'echo \\\"$sftype-$sfregion\\\"'"] = "nes-usa"
        });

        var result = await new TransportDeviceProbe(transport).ProbeAsync(CancellationToken.None);

        Assert.True(result.Found);
        Assert.Equal("AABBCC", result.HardwareId);
        Assert.Equal("nes-usa", result.ProductName);
    }

    [Fact]
    public async Task TransportProbe_Offline_ReturnsNotFoundWithoutCommands()
    {
        var transport = new FakeTransport(false, new Dictionary<string, string>());
        var result = await new TransportDeviceProbe(transport).ProbeAsync(CancellationToken.None);
        Assert.False(result.Found);
        Assert.Empty(transport.Commands);
    }

    private sealed class Probe : IDeviceProbe
    {
        private readonly DeviceProbeResult result;
        public Probe(DeviceProbeResult result) { this.result = result; }
        public Task<DeviceProbeResult> ProbeAsync(CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class FakeTransport : IDeviceCommandTransport
    {
        private readonly Dictionary<string, string> responses;
        public FakeTransport(bool isOnline, Dictionary<string, string> responses) { IsOnline = isOnline; this.responses = responses; }
        public bool IsOnline { get; }
        public List<string> Commands { get; } = new List<string>();
        public Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<string> ExecuteAsync(string command, CancellationToken ct)
        {
            Commands.Add(command);
            return Task.FromResult(responses[command]);
        }
    }
}
