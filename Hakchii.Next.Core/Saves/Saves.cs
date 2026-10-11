using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Saves
{
    public sealed class SaveSnapshot
    {
        public string GameId { get; set; }
        public byte[] Data { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public interface ISaveProvider
    {
        Task<SaveSnapshot> ReadAsync(string gameId, CancellationToken ct);
        Task<OperationResult> WriteAsync(SaveSnapshot save, CancellationToken ct);
    }

    public sealed class SaveBackupService
    {
        private readonly ISaveProvider provider;
        private readonly int maxBackupsPerGame;
        private readonly Dictionary<string, Stack<SaveSnapshot>> backups =
            new Dictionary<string, Stack<SaveSnapshot>>(StringComparer.OrdinalIgnoreCase);

        public SaveBackupService(ISaveProvider provider, int maxBackupsPerGame = 10)
        {
            this.provider = provider;
            this.maxBackupsPerGame = maxBackupsPerGame < 1 ? 1 : maxBackupsPerGame;
        }

        public async Task<OperationResult> BackupAsync(string gameId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return OperationResult.Fail("Game id is required");

            var save = await provider.ReadAsync(gameId, ct).ConfigureAwait(false);
            if (save == null)
                return OperationResult.Fail("Save not found");
            if (save.Data == null || save.Data.Length == 0)
                return OperationResult.Fail("Save data is empty");
            if (!string.Equals(save.GameId, gameId, StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Save belongs to a different game");

            if (!backups.TryGetValue(gameId, out var stack))
                backups[gameId] = stack = new Stack<SaveSnapshot>();

            stack.Push(new SaveSnapshot
            {
                GameId = save.GameId,
                Data = (byte[])save.Data.Clone(),
                CreatedUtc = DateTime.UtcNow
            });

            TrimHistory(stack);
            return OperationResult.Ok();
        }

        public int GetBackupCount(string gameId)
        {
            return backups.TryGetValue(gameId ?? string.Empty, out var stack) ? stack.Count : 0;
        }

        public async Task<OperationResult> RestoreLatestAsync(string gameId, CancellationToken ct)
        {
            if (!backups.TryGetValue(gameId, out var stack) || stack.Count == 0)
                return OperationResult.Fail("No save backup");

            return await provider.WriteAsync(stack.Peek(), ct).ConfigureAwait(false);
        }

        public async Task<OperationResult> RestorePreviousAsync(string gameId, CancellationToken ct)
        {
            if (!backups.TryGetValue(gameId, out var stack) || stack.Count == 0)
                return OperationResult.Fail("No save backup");

            var snapshot = stack.Peek();
            var result = await provider.WriteAsync(snapshot, ct).ConfigureAwait(false);
            if (result.Success)
                stack.Pop();
            return result;
        }

        private void TrimHistory(Stack<SaveSnapshot> stack)
        {
            if (stack.Count <= maxBackupsPerGame)
                return;

            var newest = stack.ToArray();
            stack.Clear();
            var keep = Math.Min(maxBackupsPerGame, newest.Length);
            for (var i = keep - 1; i >= 0; i--)
                stack.Push(newest[i]);
        }
    }

    public sealed class SaveImportGuard
    {
        private readonly SaveBackupService backups;
        private readonly ISaveProvider provider;

        public SaveImportGuard(SaveBackupService backups, ISaveProvider provider)
        {
            this.backups = backups;
            this.provider = provider;
        }

        public async Task<OperationResult> ReplaceAsync(string gameId, SaveSnapshot replacement, CancellationToken ct)
        {
            if (replacement == null)
                return OperationResult.Fail("Replacement save is required");
            if (string.IsNullOrWhiteSpace(gameId))
                return OperationResult.Fail("Game id is required");
            if (replacement.Data == null || replacement.Data.Length == 0)
                return OperationResult.Fail("Replacement save data is empty");
            if (!string.Equals(replacement.GameId, gameId, StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Replacement save belongs to a different game");

            var backup = await backups.BackupAsync(gameId, ct).ConfigureAwait(false);
            var write = await provider.WriteAsync(replacement, ct).ConfigureAwait(false);
            if (write.Success)
                return write;

            if (backup.Success)
            {
                var restore = await backups.RestoreLatestAsync(gameId, ct).ConfigureAwait(false);
                if (!restore.Success)
                    return OperationResult.Fail((write.Error ?? "write failed") + "; rollback failed: " + restore.Error);
            }

            return write;
        }
    }

    public interface ISaveEditor
    {
        OperationResult Apply(byte[] data);
    }
}
