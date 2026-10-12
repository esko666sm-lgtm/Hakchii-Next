using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Device
{
    public sealed class TransportDeviceProbe : IDeviceProbe
    {
        private readonly IDeviceCommandTransport transport;

        public TransportDeviceProbe(IDeviceCommandTransport transport)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<DeviceProbeResult> ProbeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!transport.IsOnline)
                return new DeviceProbeResult { Found = false };

            var info = new DeviceInfoService(transport);
            string hardwareId;
            try
            {
                hardwareId = await info.ReadHardwareIdAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return new DeviceProbeResult { Found = false };
            }

            string systemCode;
            try
            {
                systemCode = (await transport.ExecuteAsync("hakchi eval 'echo \\\"$sftype-$sfregion\\\"'", cancellationToken).ConfigureAwait(false)).Trim();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return new DeviceProbeResult { Found = false };
            }

            if (string.IsNullOrWhiteSpace(systemCode))
                return new DeviceProbeResult { Found = false };

            return new DeviceProbeResult
            {
                Found = true,
                HardwareId = hardwareId,
                ProductName = systemCode
            };
        }
    }
}
