using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Hakchii.Next.Core.Recovery
{
    public static class BackupManifest
    {
        public static string WriteFor(string backupPath)
            => WriteFor(backupPath, null, null);

        public static string WriteFor(string backupPath, string hardwareId, string systemCode)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                throw new ArgumentException("Backup path is required", nameof(backupPath));
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file does not exist", backupPath);

            var info = new FileInfo(backupPath);
            var manifestPath = backupPath + ".hakchii";
            var lines = new System.Collections.Generic.List<string>
            {
                "file=" + info.Name,
                "size=" + info.Length.ToString(CultureInfo.InvariantCulture),
                "sha256=" + ComputeSha256(backupPath),
                "createdUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
            if (!string.IsNullOrWhiteSpace(hardwareId)) lines.Add("hardwareId=" + hardwareId.Trim());
            if (!string.IsNullOrWhiteSpace(systemCode)) lines.Add("systemCode=" + systemCode.Trim());
            File.WriteAllLines(manifestPath, lines);
            return manifestPath;
        }

        public static OperationResult VerifyIfPresent(string backupPath)
            => VerifyIfPresent(backupPath, null, null);

        public static OperationResult VerifyIfPresent(string backupPath, string hardwareId, string systemCode)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                return OperationResult.Fail("Backup path is required");
            if (!File.Exists(backupPath))
                return OperationResult.Fail("Backup file does not exist");
            if (!File.Exists(backupPath + ".hakchii"))
                return OperationResult.Ok();

            var baseResult = VerifyFor(backupPath);
            if (!baseResult.Success) return baseResult;

            var values = ReadValues(backupPath + ".hakchii");
            if (values.TryGetValue("hardwareId", out var expectedHardwareId) &&
                !string.IsNullOrWhiteSpace(expectedHardwareId) &&
                !string.IsNullOrWhiteSpace(hardwareId) &&
                !string.Equals(expectedHardwareId.Trim(), hardwareId.Trim(), StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Backup belongs to a different console");

            if (values.TryGetValue("systemCode", out var expectedSystemCode) &&
                !string.IsNullOrWhiteSpace(expectedSystemCode) &&
                !string.IsNullOrWhiteSpace(systemCode) &&
                !string.Equals(expectedSystemCode.Trim(), systemCode.Trim(), StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Backup belongs to a different console type");

            return OperationResult.Ok();
        }

        public static OperationResult VerifyFor(string backupPath)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                return OperationResult.Fail("Backup path is required");
            if (!File.Exists(backupPath))
                return OperationResult.Fail("Backup file does not exist");

            var manifestPath = backupPath + ".hakchii";
            if (!File.Exists(manifestPath))
                return OperationResult.Fail("Backup manifest does not exist");

            var values = ReadValues(manifestPath);

            if (!values.TryGetValue("sha256", out var expectedHash))
                return OperationResult.Fail("Backup manifest has no SHA-256");
            if (!values.TryGetValue("size", out var sizeText) ||
                !long.TryParse(sizeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expectedSize))
                return OperationResult.Fail("Backup manifest has invalid size");

            var info = new FileInfo(backupPath);
            if (info.Length != expectedSize)
                return OperationResult.Fail("Backup file size mismatch");

            var actualHash = ComputeSha256(backupPath);
            if (!string.Equals(actualHash, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Backup SHA-256 mismatch");

            return OperationResult.Ok();
        }

        private static System.Collections.Generic.Dictionary<string, string> ReadValues(string manifestPath)
        {
            return File.ReadAllLines(manifestPath)
                .Select(line => line.Split(new[] { '=' }, 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
        }

        private static string ComputeSha256(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan))
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
