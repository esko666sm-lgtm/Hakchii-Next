using System;
using System.IO;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class FileIntegrityGuardSafetyTests
{
    [Fact]
    public void Capture_RejectsEmptyRecoveryFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, Array.Empty<byte>());

            var ex = Assert.Throws<InvalidDataException>(() => FileIntegrityGuard.Capture(path));

            Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Verify_LockedFile_FailsCleanly()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            var guard = FileIntegrityGuard.Capture(path);

            using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var result = guard.Verify();

            Assert.False(result.Success);
            Assert.Contains("verify", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally { File.Delete(path); }
    }
}
