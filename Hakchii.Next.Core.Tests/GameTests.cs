using Hakchii.Next.Core.Games;
using Xunit;
namespace Hakchii.Next.Core.Tests;
public class GameTests
{
    [Fact] public void LibraryAddsAndFindsGame() { var l=new GameLibrary(); var g=new GameEntry{Id="mario",Title="Mario",RomPath="/games/mario.nes",System=GameSystem.Nes}; Assert.True(l.Add(g).Success); Assert.Same(g,l.Find("MARIO")); }
    [Fact] public void LibraryRejectsDuplicateId() { var l=new GameLibrary(); l.Add(new GameEntry{Id="x",RomPath="a.nes"}); Assert.False(l.Add(new GameEntry{Id="x",RomPath="b.nes"}).Success); }
    [Fact] public void RetroArchCommandUsesAssignedCore() { var c=new LaunchCommandBuilder().Build(new GameEntry{Id="mario",RomPath="/games/Mario Bros.nes",CoreId="fceumm"}); Assert.Contains("retroarch-clover",c); Assert.Contains("fceumm",c); Assert.Contains("\"/games/Mario Bros.nes\"",c); }
}
