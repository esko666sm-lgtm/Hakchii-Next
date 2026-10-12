using Hakchii.Next.Core.Customization;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ThemeServiceSafetyTests
{
    [Fact]
    public void Register_NormalizesThemeMetadata()
    {
        var service = new ThemeService();
        var theme = new ThemePackage
        {
            Id = "  night  ",
            Name = "  Night  ",
            WallpaperPath = "  wall.png  "
        };

        var result = service.Register(theme);

        Assert.True(result.Success);
        Assert.Equal("night", theme.Id);
        Assert.Equal("Night", theme.Name);
        Assert.Equal("wall.png", theme.WallpaperPath);
        Assert.Same(theme, service.Find(" night "));
    }

    [Fact]
    public void Register_RejectsDuplicateAfterNormalization()
    {
        var service = new ThemeService();
        Assert.True(service.Register(new ThemePackage
        {
            Id = "night",
            WallpaperPath = "wall.png"
        }).Success);

        var result = service.Register(new ThemePackage
        {
            Id = " NIGHT ",
            MusicPath = "music.wav"
        });

        Assert.False(result.Success);
    }
}
