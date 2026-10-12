using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Recovery
{
    public sealed class BackupFileArtifact
    {
        public string Path { get; set; }
        public string Sha256 { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public interface IBackupFileTransport
    {
        Task<OperationResult> CreateAsync(string destinationPath, CancellationToken ct);
        Task<OperationResult> RestoreAsync(string sourcePath, CancellationToken ct);
    }

    public sealed class VerifiedFileBackupService
    {
        private readonly IBackupFileTransport transport;

        public VerifiedFileBackupService(IBackupFileTransport transport)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<BackupFileArtifact> CreateAsync(string destinationPath, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Backup destination path is required", nameof(destinationPath));

            ct.ThrowIfCancellationRequested();
            var result = await transport.CreateAsync(destinationPath, ct).ConfigureAwait(false);
            if (!result.Success)
                throw new InvalidOperationException(result.Error ?? "Backup creation failed");

            if (!File.Exists(destinationPath))
                throw new InvalidOperationException("Backup file does not exist after creation");
            if (new FileInfo(destinationPath).Length == 0)
                throw new InvalidOperationException("Backup file is empty");

            return new BackupFileArtifact
            {
                Path = destinationPath,
                Sha256 = ComputeSha256(destinationPath),
                CreatedUtc = DateTime.UtcNow
            };
        }

        public Task<OperationResult> VerifyAsync(BackupFileArtifact artifact, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (artifact == null) return Task.FromResult(OperationResult.Fail("Backup artifact is required"));
            if (string.IsNullOrWhiteSpace(artifact.Path)) return Task.FromResult(OperationResult.Fail("Backup path is required"));
            if (!File.Exists(artifact.Path)) return Task.FromResult(OperationResult.Fail("Backup file does not exist"));
            if (new FileInfo(artifact.Path).Length == 0) return Task.FromResult(OperationResult.Fail("Backup file is empty"));
            if (string.IsNullOrWhiteSpace(artifact.Sha256)) return Task.FromResult(OperationResult.Fail("Backup file has no SHA-256"));

            var actual = ComputeSha256(artifact.Path);
            if (!string.Equals(actual, artifact.Sha256.Trim(), StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(OperationResult.Fail("Backup SHA-256 mismatch"));

            return Task.FromResult(OperationResult.Ok());
        }

        public async Task<OperationResult> RestoreVerifiedAsync(BackupFileArtifact artifact, CancellationToken ct)
        {
            var verified = await VerifyAsync(artifact, ct).ConfigureAwait(false);
            if (!verified.Success) return verified;
            return await transport.RestoreAsync(artifact.Path, ct).ConfigureAwait(false);
        }

        private static string ComputeSha256(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan))
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
