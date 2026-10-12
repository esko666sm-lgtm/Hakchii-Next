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

        public bool IsConnected => transport.IsOnline && Profile != null;
        public ConsoleProfile Profile { get; private set; }

        public async Task<OperationResult> ConnectAsync(CancellationToken cancellationToken)
        {
            Profile = null;

            try
            {
                await transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
                if (!transport.IsOnline)
                    return OperationResult.Fail("CloverShell transport did not come online");

                var detected = await detector.DetectAsync(cancellationToken).ConfigureAwait(false);
                if (detected == null)
                {
                    await SafeDisconnectAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult.Fail("No supported console detected");
                }

                if (!detected.Capabilities.SupportsCloverShell)
                {
                    await SafeDisconnectAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult.Fail("Console does not support CloverShell");
                }

                Profile = detected;
                return OperationResult.Ok();
            }
            catch (OperationCanceledException)
            {
                await SafeDisconnectAsync(CancellationToken.None).ConfigureAwait(false);
                Profile = null;
                throw;
            }
            catch (Exception ex)
            {
                await SafeDisconnectAsync(CancellationToken.None).ConfigureAwait(false);
                Profile = null;
                return OperationResult.Fail(ex.Message);
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            await transport.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            Profile = null;
        }

        private async Task SafeDisconnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (transport.IsOnline)
                    await transport.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
            }
            finally
            {
                Profile = null;
            }
        }
    }
}
