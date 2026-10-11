using Hakchii.Next.Core.Device;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace com.clusterrr.hakchi_gui.Next
{
    internal sealed class SystemShellTransportAdapter : IDeviceCommandTransport
    {
        private readonly ISystemShell shell;
        public SystemShellTransportAdapter(ISystemShell shell) { this.shell = shell ?? throw new ArgumentNullException(nameof(shell)); }
        public bool IsOnline => shell.IsOnline;
        public Task ConnectAsync(CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); shell.Connect(); return Task.CompletedTask; }
        public Task DisconnectAsync(CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); shell.Disconnect(); return Task.CompletedTask; }
        public Task<string> ExecuteAsync(string command, CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); return Task.Run(() => shell.ExecuteSimple(command, 5000, true), cancellationToken); }
    }
}
