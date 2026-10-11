using System;
using System.IO;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class BackupManifestTests
{
    [Fact]
    public void WriteFor_CreatesManifestWithHashSizeAndTimestamp()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            var manifestPath = BackupManifest.WriteFor(path);

            Assert.True(File.Exists(manifestPath));
            var text = File.ReadAllText(manifestPath);
            Assert.Contains("sha256=", text);
            Assert.Contains("size=4", text);
            Assert.Contains("createdUtc=", text);
            Assert.Contains("file=" + Path.GetFileName(path), text);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }

    [Fact]
    public void VerifyFor_UnchangedFile_Passes()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            BackupManifest.WriteFor(path);

            var result = BackupManifest.VerifyFor(path);

            Assert.True(result.Success);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }

    [Fact]
    public void VerifyFor_ModifiedFile_Fails()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
            BackupManifest.WriteFor(path);
            File.WriteAllBytes(path, new byte[] { 9, 2, 3, 4 });

            var result = BackupManifest.VerifyFor(path);

            Assert.False(result.Success);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }
}
