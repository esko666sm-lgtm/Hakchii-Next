using System;
using System.IO;
using Hakchii.Next.Core.Recovery;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class BackupManifestDeviceBindingTests
{
    [Fact]
    public void WriteFor_WithDeviceIdentity_StoresIdentity()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path, "ABC123", "nes-usa");
            var text = File.ReadAllText(path + ".hakchii");
            Assert.Contains("hardwareId=ABC123", text);
            Assert.Contains("systemCode=nes-usa", text);
        }
        finally { File.Delete(path); File.Delete(path + ".hakchii"); }
    }

    [Fact]
    public void VerifyIfPresent_MatchingDevice_Passes()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path, "ABC123", "nes-usa");
            var result = BackupManifest.VerifyIfPresent(path, "abc123", "NES-USA");
            Assert.True(result.Success);
        }
        finally { File.Delete(path); File.Delete(path + ".hakchii"); }
    }

    [Fact]
    public void VerifyIfPresent_DifferentHardwareId_Fails()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path, "ABC123", "nes-usa");
            var result = BackupManifest.VerifyIfPresent(path, "OTHER", "nes-usa");
            Assert.False(result.Success);
        }
        finally { File.Delete(path); File.Delete(path + ".hakchii"); }
    }

    [Fact]
    public void VerifyIfPresent_DifferentSystemCode_Fails()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path, "ABC123", "nes-usa");
            var result = BackupManifest.VerifyIfPresent(path, "ABC123", "snes-eur");
            Assert.False(result.Success);
        }
        finally { File.Delete(path); File.Delete(path + ".hakchii"); }
    }

    [Fact]
    public void VerifyIfPresent_LegacyManifestWithoutIdentity_RemainsCompatible()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            BackupManifest.WriteFor(path);
            var result = BackupManifest.VerifyIfPresent(path, "ABC123", "nes-usa");
            Assert.True(result.Success);
        }
        finally { File.Delete(path); File.Delete(path + ".hakchii"); }
    }
}
