using System;
using System.IO;
using Hakchii.Next.Core.Customization;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ThemePackageTests
{
    [Fact]
    public void Register_RejectsThemeWithoutAssets()
    {
        var service = new ThemeService();
        var result = service.Register(new ThemePackage { Id = "empty" });
        Assert.False(result.Success);
    }

    [Fact]
    public void Register_RejectsMissingWallpaper()
    {
        var service = new ThemeService();
        var result = service.Register(new ThemePackage
        {
            Id = "bad",
            WallpaperPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png")
        });
        Assert.False(result.Success);
    }

    [Fact]
    public void Register_AcceptsExistingWallpaperAndMusic()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var wallpaper = Path.Combine(dir, "wall.png");
        var music = Path.Combine(dir, "music.ogg");
        File.WriteAllBytes(wallpaper, new byte[] { 1 });
        File.WriteAllBytes(music, new byte[] { 2 });

        try
        {
            var service = new ThemeService();
            var result = service.Register(new ThemePackage
            {
                Id = "blue",
                Name = "Blue Theme",
                WallpaperPath = wallpaper,
                MusicPath = music
            });

            Assert.True(result.Success);
            Assert.Equal("Blue Theme", service.Find("BLUE").Name);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ThemePackageManifest_RoundTrips()
    {
        var dir = Path.Combine(Path.GetTempPath(), "hakchii-theme-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "theme.hakchii-theme");

        try
        {
            var store = new ThemePackageStore();
            var theme = new ThemePackage
            {
                Id = "night",
                Name = "Night",
                WallpaperPath = "wall.png",
                MusicPath = "music.ogg"
            };

            Assert.True(store.SaveManifest(path, theme).Success);
            var loaded = store.LoadManifest(path);

            Assert.Equal("night", loaded.Id);
            Assert.Equal("Night", loaded.Name);
            Assert.Equal("wall.png", loaded.WallpaperPath);
            Assert.Equal("music.ogg", loaded.MusicPath);
        }
        finally { Directory.Delete(dir, true); }
    }
}
