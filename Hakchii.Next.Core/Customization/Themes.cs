using System; using System.Collections.Generic;
namespace Hakchii.Next.Core.Customization {
 public sealed class ThemePackage { public string Id{get;set;} public string WallpaperPath{get;set;} public string MusicPath{get;set;} }
 public interface IThemeService { OperationResult Register(ThemePackage theme); ThemePackage Find(string id); }
 public sealed class ThemeService:IThemeService { private readonly Dictionary<string,ThemePackage> themes=new Dictionary<string,ThemePackage>(StringComparer.OrdinalIgnoreCase); public OperationResult Register(ThemePackage t){if(t==null||string.IsNullOrWhiteSpace(t.Id))return OperationResult.Fail("Theme id is required");if(themes.ContainsKey(t.Id))return OperationResult.Fail("Theme already exists");themes[t.Id]=t;return OperationResult.Ok();} public ThemePackage Find(string id){themes.TryGetValue(id??"",out var t);return t;} }
}
