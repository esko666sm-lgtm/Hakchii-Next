using System;
using System.IO;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class BackupManifestSafetyTests
{
    [Fact]
    public void VerifyFor_RejectsManifestBoundToDifferentFileName()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-manifest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var original = Path.Combine(dir, "original.bin");
        var renamed = Path.Combine(dir, "renamed.bin");

        try
        {
            File.WriteAllBytes(original, new byte[] { 1, 2, 3, 4 });
            BackupManifest.WriteFor(original);
            File.Move(original, renamed);
            File.Move(original + ".hakchii", renamed + ".hakchii");

            var result = BackupManifest.VerifyFor(renamed);

            Assert.False(result.Success);
            Assert.Contains("file", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void VerifyFor_DuplicateManifestKey_FailsCleanly()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path);
            File.AppendAllText(path + ".hakchii", Environment.NewLine + "size=3");

            var result = BackupManifest.VerifyFor(path);

            Assert.False(result.Success);
            Assert.Contains("manifest", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }
}
