using System;
using System.IO;
using Hakchii.Next.Core.Customization;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ThemePackageResolverTests
{
    [Fact]
    public void LoadResolved_ResolvesRelativeAssetsAgainstManifestDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        var wall = Path.Combine(dir, "wall.png");
        var music = Path.Combine(dir, "music.ogg");
        File.WriteAllBytes(wall, new byte[] { 1 });
        File.WriteAllBytes(music, new byte[] { 2 });
        File.WriteAllLines(manifest, new[]
        {
            "id=night",
            "name=Night",
            "wallpaper=wall.png",
            "music=music.ogg"
        });

        try
        {
            var result = new ThemePackageStore().LoadResolved(manifest);

            Assert.True(result.Success);
            Assert.Equal(Path.GetFullPath(wall), result.Value.WallpaperPath);
            Assert.Equal(Path.GetFullPath(music), result.Value.MusicPath);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadResolved_RejectsMissingRelativeAsset()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        File.WriteAllLines(manifest, new[] { "id=bad", "wallpaper=missing.png" });

        try
        {
            var result = new ThemePackageStore().LoadResolved(manifest);
            Assert.False(result.Success);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void LoadResolved_RejectsRelativePathEscapingPackageDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        var dir = Path.Combine(root, "package");
        Directory.CreateDirectory(dir);
        var outside = Path.Combine(root, "outside.png");
        File.WriteAllBytes(outside, new byte[] { 1 });
        var manifest = Path.Combine(dir, "theme.hakchii-theme");
        File.WriteAllLines(manifest, new[] { "id=escape", "wallpaper=..\\outside.png" });

        try
        {
            var result = new ThemePackageStore().LoadResolved(manifest);
            Assert.False(result.Success);
        }
        finally { Directory.Delete(root, true); }
    }
}
