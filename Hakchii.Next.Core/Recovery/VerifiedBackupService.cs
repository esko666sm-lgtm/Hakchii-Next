using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Recovery
{
    public interface IBackupTransport
    {
        Task<byte[]> ReadAsync(CancellationToken ct);
        Task<OperationResult> WriteAsync(byte[] backupData, CancellationToken ct);
    }

    public sealed class VerifiedBackupService : IBackupService
    {
        private readonly IBackupTransport transport;

        public VerifiedBackupService(IBackupTransport transport)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<BackupImage> CreateAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var data = await transport.ReadAsync(ct).ConfigureAwait(false);
            if (data == null || data.Length == 0)
                throw new InvalidOperationException("Backup transport returned no data");

            return new BackupImage
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = data,
                Sha256 = ComputeSha256(data),
                CreatedUtc = DateTime.UtcNow
            };
        }

        public Task<OperationResult> VerifyAsync(BackupImage image, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (image == null) return Task.FromResult(OperationResult.Fail("Backup image is required"));
            if (image.Data == null || image.Data.Length == 0) return Task.FromResult(OperationResult.Fail("Backup image has no data"));
            if (string.IsNullOrWhiteSpace(image.Sha256)) return Task.FromResult(OperationResult.Fail("Backup image has no SHA-256"));

            var actual = ComputeSha256(image.Data);
            if (!string.Equals(actual, image.Sha256.Trim(), StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(OperationResult.Fail("Backup SHA-256 mismatch"));

            return Task.FromResult(OperationResult.Ok());
        }

        public async Task<OperationResult> RestoreAsync(BackupImage image, CancellationToken ct)
        {
            var verified = await VerifyAsync(image, ct).ConfigureAwait(false);
            if (!verified.Success) return verified;
            return await transport.WriteAsync(image.Data, ct).ConfigureAwait(false);
        }

        private static string ComputeSha256(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
