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

    private sealed class Probe : IDeviceProbe
    {
        private readonly DeviceProbeResult result;
        public Probe(DeviceProbeResult result) { this.result = result; }
        public Task<DeviceProbeResult> ProbeAsync(CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
