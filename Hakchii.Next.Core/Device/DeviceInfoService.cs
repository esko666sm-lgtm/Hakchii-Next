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
            this.transport = transport;
        }

        public async Task<string> ReadHardwareIdAsync(CancellationToken ct)
        {
            return (await transport.ExecuteAsync("hakchi hwid", ct).ConfigureAwait(false)).Replace(" ", "");
        }

        public async Task<DeviceInfo> ReadDetailsAsync(string hardwareId, CancellationToken ct)
        {
            return new DeviceInfo
            {
                HardwareId = hardwareId,
                SystemCode = (await transport.ExecuteAsync("hakchi eval 'echo \\\"$sftype-$sfregion\\\"'", ct).ConfigureAwait(false)).Trim(),
                GameStoragePath = (await transport.ExecuteAsync("hakchi findGameSyncStorage", ct).ConfigureAwait(false)).Trim(),
                OriginalGamesPath = (await transport.ExecuteAsync("hakchi get gamepath", ct).ConfigureAwait(false)).Trim(),
                RootFsPath = (await transport.ExecuteAsync("hakchi get rootfs", ct).ConfigureAwait(false)).Trim()
            };
        }

        public async Task<DeviceInfo> ReadAsync(CancellationToken ct)
        {
            var hardwareId = await ReadHardwareIdAsync(ct).ConfigureAwait(false);
            return await ReadDetailsAsync(hardwareId, ct).ConfigureAwait(false);
        }
    }
}
