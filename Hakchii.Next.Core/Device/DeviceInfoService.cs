using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Device
{
    public interface IDeviceCommandTransport : ICloverShellTransport
    {
        Task<string> ExecuteAsync(string command, CancellationToken ct);
    }

    public sealed class DeviceInfo
    {
        public string HardwareId { get; set; }
        public string SystemCode { get; set; }
        public string GameStoragePath { get; set; }
        public string OriginalGamesPath { get; set; }
        public string RootFsPath { get; set; }
    }

    public sealed class DeviceInfoService
    {
        private readonly IDeviceCommandTransport transport;

        public DeviceInfoService(IDeviceCommandTransport transport)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<string> ReadHardwareIdAsync(CancellationToken ct)
        {
            var raw = await transport.ExecuteAsync("hakchi hwid", ct).ConfigureAwait(false);
            var normalized = new string((raw ?? string.Empty).Where(ch => !char.IsWhiteSpace(ch)).ToArray());

            if (string.IsNullOrWhiteSpace(normalized))
                throw new InvalidOperationException("Device hardware id is empty");

            return normalized;
        }

        public async Task<DeviceInfo> ReadDetailsAsync(string hardwareId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(hardwareId))
                throw new InvalidOperationException("Device hardware id is empty");

            hardwareId = hardwareId.Trim();

            var rawSystemCode = await transport.ExecuteAsync("hakchi eval 'echo \"$sftype-$sfregion\"'", ct).ConfigureAwait(false);
            var systemCode = (rawSystemCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(systemCode))
                throw new InvalidOperationException("Device system code is empty");

            var rawGameStoragePath = await transport.ExecuteAsync("hakchi findGameSyncStorage", ct).ConfigureAwait(false);
            var gameStoragePath = RequireAbsoluteUnixPath((rawGameStoragePath ?? string.Empty).Trim(), "Game storage path");
            var rawOriginalGamesPath = await transport.ExecuteAsync("hakchi get gamepath", ct).ConfigureAwait(false);
            var originalGamesPath = RequireAbsoluteUnixPath((rawOriginalGamesPath ?? string.Empty).Trim(), "Original games path");
            var rawRootFsPath = await transport.ExecuteAsync("hakchi get rootfs", ct).ConfigureAwait(false);
            var rootFsPath = RequireAbsoluteUnixPath((rawRootFsPath ?? string.Empty).Trim(), "Rootfs path");

            return new DeviceInfo
            {
                HardwareId = hardwareId,
                SystemCode = systemCode,
                GameStoragePath = gameStoragePath,
                OriginalGamesPath = originalGamesPath,
                RootFsPath = rootFsPath
            };
        }

        public async Task<DeviceInfo> ReadAsync(CancellationToken ct)
        {
            var hardwareId = await ReadHardwareIdAsync(ct).ConfigureAwait(false);
            return await ReadDetailsAsync(hardwareId, ct).ConfigureAwait(false);
        }

        private static string RequireAbsoluteUnixPath(string path, string label)
        {
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/", StringComparison.Ordinal))
                throw new InvalidOperationException(label + " is invalid");
            return path;
        }
    }
}
