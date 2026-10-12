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
            var product = (probe.ProductName ?? string.Empty).Trim().ToLowerInvariant();
            var hardware = (probe.HardwareId ?? string.Empty).Trim().ToLowerInvariant();

            var family = IsSnes(product) ? ConsoleFamily.SnesMini
                : IsNes(product, hardware) ? ConsoleFamily.NesMini
                : ConsoleFamily.Unknown;
            return new ConsoleProfile
            {
                Id = probe.HardwareId,
                Name = string.IsNullOrWhiteSpace(probe.ProductName) ? "Unknown console" : probe.ProductName,
                Family = family,
                Capabilities = CapabilitiesFor(family)
            };
        }

        private static bool IsSnes(string product)
        {
            return product == "snes" || product.StartsWith("snes-") ||
                   product.Contains("classic mini: snes") ||
                   product.Contains("super famicom");
        }

        private static bool IsNes(string product, string hardware)
        {
            return product == "nes" || product.StartsWith("nes-") ||
                   product.Contains("classic mini: nes") ||
                   product == "famicom" || product.Contains("classic mini: famicom") ||
                   hardware.Contains("vid_1f3a") || hardware.Contains("1f3a");
        }

        public static DeviceCapabilities CapabilitiesFor(ConsoleFamily family)
        {
            var supported = family == ConsoleFamily.NesMini || family == ConsoleFamily.SnesMini;
            return new DeviceCapabilities { SupportsCloverShell = supported, SupportsHmod = supported, SupportsRetroArch = supported, SupportsNandBackup = supported };
        }
    }
}
