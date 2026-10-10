using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Device
{
    public interface IDeviceProbe
    {
        Task<DeviceProbeResult> ProbeAsync(CancellationToken cancellationToken);
    }

    public sealed class DeviceProbeResult
    {
        public bool Found { get; set; }
        public string HardwareId { get; set; }
        public string ProductName { get; set; }
    }

    public sealed class DeviceDetector : IDeviceDetector
    {
        private readonly IDeviceProbe probe;
        public DeviceDetector(IDeviceProbe probe) { this.probe = probe; }

        public async Task<ConsoleProfile> DetectAsync(CancellationToken cancellationToken)
        {
            var result = await probe.ProbeAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || !result.Found) return null;
            return ConsoleProfiles.FromProbe(result);
        }
    }

    public static class ConsoleProfiles
    {
        public static ConsoleProfile FromProbe(DeviceProbeResult probe)
        {
            var text = ((probe.ProductName ?? "") + " " + (probe.HardwareId ?? "")).ToLowerInvariant();
            var family = text.Contains("snes") || text.Contains("super famicom") ? ConsoleFamily.SnesMini
                : text.Contains("nes") || text.Contains("famicom") || text.Contains("1f3a") ? ConsoleFamily.NesMini
                : ConsoleFamily.Unknown;
            return new ConsoleProfile
            {
                Id = probe.HardwareId,
                Name = string.IsNullOrWhiteSpace(probe.ProductName) ? "Unknown console" : probe.ProductName,
                Family = family,
                Capabilities = CapabilitiesFor(family)
            };
        }

        public static DeviceCapabilities CapabilitiesFor(ConsoleFamily family)
        {
            var supported = family == ConsoleFamily.NesMini || family == ConsoleFamily.SnesMini;
            return new DeviceCapabilities { SupportsCloverShell = supported, SupportsHmod = supported, SupportsRetroArch = supported, SupportsNandBackup = supported };
        }
    }
}
