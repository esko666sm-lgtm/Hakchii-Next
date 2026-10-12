using System;
using System.IO;
using System.Security.Cryptography;

namespace Hakchii.Next.Core.Recovery
{
    public sealed class FileIntegrityGuard
    {
        private readonly string path;
        private readonly string sha256;

        private FileIntegrityGuard(string path, string sha256)
        {
            this.path = path;
            this.sha256 = sha256;
        }

        public static FileIntegrityGuard Capture(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is required", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("File does not exist", path);
            if (new FileInfo(path).Length == 0)
                throw new InvalidDataException("Recovery file is empty");

            return new FileIntegrityGuard(path, ComputeSha256(path));
        }

        public OperationResult Verify()
        {
            if (!File.Exists(path))
                return OperationResult.Fail("Selected recovery file no longer exists");

            string current;
            try
            {
                current = ComputeSha256(path);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return OperationResult.Fail("Unable to verify selected recovery file: " + ex.Message);
            }

            if (!string.Equals(current, sha256, StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Selected recovery file changed after validation");

            return OperationResult.Ok();
        }

        private static string ComputeSha256(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan))
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
