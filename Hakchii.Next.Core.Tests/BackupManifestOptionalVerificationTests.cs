using System;
using System.IO;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class BackupManifestOptionalVerificationTests
{
    [Fact]
    public void VerifyIfPresent_NoManifest_AllowsLegacyBackup()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            var result = BackupManifest.VerifyIfPresent(path);
            Assert.True(result.Success);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }

    [Fact]
    public void VerifyIfPresent_ValidManifest_Passes()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path);
            var result = BackupManifest.VerifyIfPresent(path);
            Assert.True(result.Success);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }

    [Fact]
    public void VerifyIfPresent_InvalidManifest_Fails()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path);
            File.WriteAllBytes(path, new byte[] { 9, 2, 3 });
            var result = BackupManifest.VerifyIfPresent(path);
            Assert.False(result.Success);
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".hakchii");
        }
    }
}
