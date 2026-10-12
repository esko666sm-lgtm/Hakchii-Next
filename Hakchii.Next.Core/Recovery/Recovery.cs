using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Recovery
{
    public sealed class BackupImage
    {
        public string Id { get; set; }
        public byte[] Data { get; set; }
        public string Sha256 { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public interface IBackupService
    {
        Task<BackupImage> CreateAsync(CancellationToken ct);
        Task<OperationResult> VerifyAsync(BackupImage image, CancellationToken ct);
        Task<OperationResult> RestoreAsync(BackupImage image, CancellationToken ct);
    }

    public sealed class RecoveryService
    {
        private readonly IBackupService backup;

        public RecoveryService(IBackupService backup)
        {
            this.backup = backup ?? throw new ArgumentNullException(nameof(backup));
        }

        public async Task<OperationResult> RestoreVerifiedAsync(BackupImage image, CancellationToken ct)
        {
            if (image == null)
                return OperationResult.Fail("Backup image is required");

            OperationResult verified;
            try
            {
                verified = await backup.VerifyAsync(image, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("Backup verification failed: " + ex.Message);
            }

            if (verified == null)
                return OperationResult.Fail("Backup verification returned no result");
            if (!verified.Success)
                return verified;

            try
            {
                var restored = await backup.RestoreAsync(image, ct).ConfigureAwait(false);
                return restored ?? OperationResult.Fail("Backup restore returned no result");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("Backup restore failed: " + ex.Message);
            }
        }
    }
}
