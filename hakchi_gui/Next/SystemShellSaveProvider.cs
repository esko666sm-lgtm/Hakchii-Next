using Hakchii.Next.Core;
using Hakchii.Next.Core.Saves;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace com.clusterrr.hakchi_gui.Next
{
    internal sealed class SystemShellSaveProvider : ISaveProvider
    {
        private readonly ISystemShell shell;

        public SystemShellSaveProvider(ISystemShell shell)
        {
            this.shell = shell ?? throw new ArgumentNullException(nameof(shell));
        }

        public Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var output = new MemoryStream())
                {
                    var id = Shared.EscapeShellArgument(gameId);
                    var rc = shell.Execute($"cd /var/lib/clover/profiles/0 && [ -d {id} ] && tar -cz {id}", null, output, null, 10000, false);
                    if (rc != 0)
                        return null;
                    return new SaveSnapshot { GameId = gameId, Data = output.ToArray(), CreatedUtc = DateTime.UtcNow };
                }
            }, cancellationToken);
        }

        public Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (save == null || save.Data == null || save.Data.Length == 0)
                return Task.FromResult(OperationResult.Fail("Save data is required"));

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var input = new MemoryStream(save.Data, writable: false))
                {
                    var rc = shell.Execute("cd /var/lib/clover/profiles/0 && tar -xz", input, null, null, 10000, false);
                    return rc == 0 ? OperationResult.Ok() : OperationResult.Fail("Save import failed");
                }
            }, cancellationToken);
        }
    }
}
