using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hakchii.Next.Core.Customization
{
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
    }
}
