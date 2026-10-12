using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hakchii.Next.Core.Customization
{
    public sealed class ThemePackageLoadResult
    {
        public bool Success { get; set; }
        public ThemePackage Value { get; set; }
        public string Error { get; set; }

        public static ThemePackageLoadResult Ok(ThemePackage value)
            => new ThemePackageLoadResult { Success = true, Value = value };

        public static ThemePackageLoadResult Fail(string error)
            => new ThemePackageLoadResult { Success = false, Error = error };
    }

    public sealed class ThemePackageStore
    {
        public OperationResult SaveManifest(string path, ThemePackage theme)
        {
            if (string.IsNullOrWhiteSpace(path)) return OperationResult.Fail("Theme manifest path is required");
            if (theme == null || string.IsNullOrWhiteSpace(theme.Id)) return OperationResult.Fail("Theme is invalid");

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

                var lines = new List<string>
                {
                    "id=" + theme.Id,
                    "name=" + (theme.Name ?? string.Empty),
                    "wallpaper=" + (theme.WallpaperPath ?? string.Empty),
                    "music=" + (theme.MusicPath ?? string.Empty)
                };
                File.WriteAllLines(path, lines);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message);
            }
        }

        public ThemePackage LoadManifest(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;

            var values = File.ReadAllLines(path)
                .Select(line => line.Split(new[] { '=' }, 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

            values.TryGetValue("id", out var id);
            values.TryGetValue("name", out var name);
            values.TryGetValue("wallpaper", out var wallpaper);
            values.TryGetValue("music", out var music);

            if (string.IsNullOrWhiteSpace(id)) return null;

            return new ThemePackage
            {
                Id = id,
                Name = name,
                WallpaperPath = wallpaper,
                MusicPath = music
            };
        }

        public ThemePackageLoadResult LoadResolved(string manifestPath)
        {
            var theme = LoadManifest(manifestPath);
            if (theme == null)
                return ThemePackageLoadResult.Fail("Theme manifest is missing or invalid");

            var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath));
            if (string.IsNullOrWhiteSpace(baseDirectory))
                return ThemePackageLoadResult.Fail("Theme manifest directory is invalid");

            var wallpaperResult = ResolveAsset(baseDirectory, theme.WallpaperPath, "Wallpaper");
            if (!wallpaperResult.Success) return ThemePackageLoadResult.Fail(wallpaperResult.Error);

            var musicResult = ResolveAsset(baseDirectory, theme.MusicPath, "Music");
            if (!musicResult.Success) return ThemePackageLoadResult.Fail(musicResult.Error);

            theme.WallpaperPath = wallpaperResult.Path;
            theme.MusicPath = musicResult.Path;

            return ThemePackageLoadResult.Ok(theme);
        }

        private static AssetResolveResult ResolveAsset(string baseDirectory, string assetPath, string label)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return AssetResolveResult.Ok(null);

            var fullPath = Path.IsPathRooted(assetPath)
                ? Path.GetFullPath(assetPath)
                : Path.GetFullPath(Path.Combine(baseDirectory, assetPath));

            if (!Path.IsPathRooted(assetPath))
            {
                var baseWithSeparator = baseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(baseWithSeparator, StringComparison.OrdinalIgnoreCase))
                    return AssetResolveResult.Fail(label + " path escapes theme package directory");
            }

            if (!File.Exists(fullPath))
                return AssetResolveResult.Fail(label + " file does not exist");

            return AssetResolveResult.Ok(fullPath);
        }

        private sealed class AssetResolveResult
        {
            public bool Success { get; private set; }
            public string Path { get; private set; }
            public string Error { get; private set; }

            public static AssetResolveResult Ok(string path)
                => new AssetResolveResult { Success = true, Path = path };

            public static AssetResolveResult Fail(string error)
                => new AssetResolveResult { Success = false, Error = error };
        }
    }
}
