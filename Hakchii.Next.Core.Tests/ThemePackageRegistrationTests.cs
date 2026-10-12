using System;
using System.IO;
using Hakchii.Next.Core.Customization;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ThemePackageRegistrationTests
{
    [Fact]
    public void RegisterPackage_LoadsResolvesAndRegistersTheme()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        var wall = Path.Combine(dir, "wall.png");
        File.WriteAllBytes(wall, new byte[] { 1 });
        File.WriteAllLines(manifest, new[] { "id=blue", "name=Blue", "wallpaper=wall.png" });

        try
        {
            var service = new ThemeService();
            var result = service.RegisterPackage(manifest, new ThemePackageStore());

            Assert.True(result.Success);
            Assert.Equal(Path.GetFullPath(wall), service.Find("BLUE").WallpaperPath);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void RegisterPackage_InvalidManifest_DoesNotRegister()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        File.WriteAllLines(manifest, new[] { "name=MissingId", "wallpaper=missing.png" });

        try
        {
            var service = new ThemeService();
            var result = service.RegisterPackage(manifest, new ThemePackageStore());

            Assert.False(result.Success);
            Assert.Null(service.Find("MissingId"));
        }
        finally { Directory.Delete(dir, true); }
    }
}
