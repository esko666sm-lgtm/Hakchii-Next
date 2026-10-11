using System;
using System.IO;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class FileIntegrityGuardTests
{
    [Fact]
    public void CaptureAndVerify_UnchangedFile_Passes()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            var guard = FileIntegrityGuard.Capture(path);

            var result = guard.Verify();

            Assert.True(result.Success);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Verify_ModifiedFile_Fails()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            var guard = FileIntegrityGuard.Capture(path);
            File.WriteAllBytes(path, new byte[] { 9, 2, 3, 4 });

            var result = guard.Verify();

            Assert.False(result.Success);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Verify_MissingFile_Fails()
    {
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
        var guard = FileIntegrityGuard.Capture(path);
        File.Delete(path);

        var result = guard.Verify();

        Assert.False(result.Success);
    }
}
