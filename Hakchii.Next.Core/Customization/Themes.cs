using System;
using System.Collections.Generic;
using System.IO;

namespace Hakchii.Next.Core.Customization
{
    public sealed class ThemePackage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string WallpaperPath { get; set; }
        public string MusicPath { get; set; }
    }

    public interface IThemeService
    {
        OperationResult Register(ThemePackage theme);
        ThemePackage Find(string id);
    }

    public sealed class ThemeService : IThemeService
    {
        private readonly Dictionary<string, ThemePackage> themes =
            new Dictionary<string, ThemePackage>(StringComparer.OrdinalIgnoreCase);

        public OperationResult Register(ThemePackage theme)
        {
            if (theme == null || string.IsNullOrWhiteSpace(theme.Id))
                return OperationResult.Fail("Theme id is required");

            if (string.IsNullOrWhiteSpace(theme.WallpaperPath) &&
                string.IsNullOrWhiteSpace(theme.MusicPath))
                return OperationResult.Fail("Theme requires at least one asset");

            if (!string.IsNullOrWhiteSpace(theme.WallpaperPath) &&
                Path.IsPathRooted(theme.WallpaperPath) &&
                !File.Exists(theme.WallpaperPath))
                return OperationResult.Fail("Wallpaper file does not exist");

            if (!string.IsNullOrWhiteSpace(theme.MusicPath) &&
                Path.IsPathRooted(theme.MusicPath) &&
                !File.Exists(theme.MusicPath))
                return OperationResult.Fail("Music file does not exist");

            if (themes.ContainsKey(theme.Id))
                return OperationResult.Fail("Theme already exists");

            themes[theme.Id] = theme;
            return OperationResult.Ok();
        }

        public ThemePackage Find(string id)
        {
            themes.TryGetValue(id ?? string.Empty, out var theme);
            return theme;
        }
    }
}
