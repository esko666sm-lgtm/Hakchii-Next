using System;
using System.Collections.Generic;
using System.Linq;
using Hakchii.Next.Core.Games;
namespace Hakchii.Next.Core.Cheats {
 public sealed class CheatEntry { public string Id{get;set;} public string GameId{get;set;} public string Name{get;set;} public string Code{get;set;} public bool Enabled{get;set;} }
 public interface ICheatProvider { IReadOnlyCollection<CheatEntry> Find(GameEntry game); }
 public sealed class CheatDatabase : ICheatProvider { private readonly List<CheatEntry> items=new List<CheatEntry>(); public OperationResult Add(CheatEntry c){if(c==null||string.IsNullOrWhiteSpace(c.Id)||string.IsNullOrWhiteSpace(c.GameId)||string.IsNullOrWhiteSpace(c.Code))return OperationResult.Fail("Invalid cheat"); if(items.Any(x=>string.Equals(x.Id,c.Id,StringComparison.OrdinalIgnoreCase)))return OperationResult.Fail("Cheat already exists");items.Add(c);return OperationResult.Ok();} public IReadOnlyCollection<CheatEntry> Find(GameEntry g)=>g==null?new CheatEntry[0]:items.Where(x=>string.Equals(x.GameId,g.Id,StringComparison.OrdinalIgnoreCase)).ToArray(); }
 public sealed class RetroArchCheatSerializer { public string Serialize(IEnumerable<CheatEntry> cheats){var a=(cheats??new CheatEntry[0]).ToArray();var lines=new List<string>{"cheats = \""+a.Length+"\""};for(int i=0;i<a.Length;i++){lines.Add($"cheat{i}_desc = \"{Esc(a[i].Name)}\"");lines.Add($"cheat{i}_code = \"{Esc(a[i].Code)}\"");lines.Add($"cheat{i}_enable = \"{a[i].Enabled.ToString().ToLowerInvariant()}\"");}return string.Join("\n",lines);} private static string Esc(string s)=>(s??"").Replace("\\","\\\\").Replace("\"","\\\""); }
}
