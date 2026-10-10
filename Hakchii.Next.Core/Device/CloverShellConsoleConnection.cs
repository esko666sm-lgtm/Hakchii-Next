using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Device
{
    public interface ICloverShellTransport
    {
        bool IsOnline { get; }
        Task ConnectAsync(CancellationToken cancellationToken);
        Task DisconnectAsync(CancellationToken cancellationToken);
    }

    public sealed class CloverShellConsoleConnection : IConsoleConnection
    {
        private readonly ICloverShellTransport transport;
        private readonly IDeviceDetector detector;

        public CloverShellConsoleConnection(ICloverShellTransport transport, IDeviceDetector detector)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        public bool IsConnected => transport.IsOnline;
        public ConsoleProfile Profile { get; private set; }

        public async Task<OperationResult> ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                Profile = await detector.DetectAsync(cancellationToken).ConfigureAwait(false);
                if (Profile == null) return OperationResult.Fail("No supported console detected");
                if (!Profile.Capabilities.SupportsCloverShell) return OperationResult.Fail("Console does not support CloverShell");
                await transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
                return transport.IsOnline ? OperationResult.Ok() : OperationResult.Fail("CloverShell transport did not come online");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { return OperationResult.Fail(ex.Message); }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            await transport.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
