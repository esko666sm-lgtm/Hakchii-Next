using System;
using System.IO;
using Hakchii.Next.Core.Customization;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ThemeManifestSafetyTests
{
    [Fact]
    public void LoadResolved_DuplicateKey_FailsCleanly()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        File.WriteAllLines(manifest, new[]
        {
            "id=one",
            "id=two"
        });

        try
        {
            var result = new ThemePackageStore().LoadResolved(manifest);

            Assert.False(result.Success);
            Assert.Contains("manifest", result.Error, StringComparison.OrdinalIgnoreCase);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadResolved_NormalizesManifestValues()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        var wall = Path.Combine(dir, "wall.png");
        File.WriteAllBytes(wall, new byte[] { 1 });
        File.WriteAllLines(manifest, new[]
        {
            " id = night ",
            " name = Night ",
            " wallpaper = wall.png "
        });

        try
        {
            var result = new ThemePackageStore().LoadResolved(manifest);

            Assert.True(result.Success);
            Assert.Equal("night", result.Value.Id);
            Assert.Equal("Night", result.Value.Name);
            Assert.Equal(Path.GetFullPath(wall), result.Value.WallpaperPath);
        }
        finally { Directory.Delete(dir, true); }
    }
}
