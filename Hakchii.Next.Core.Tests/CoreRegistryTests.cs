using Hakchii.Next.Core.Emulation;
using Hakchii.Next.Core.Games;
using Xunit;
namespace Hakchii.Next.Core.Tests;
public class CoreRegistryTests
{
    private static CoreRegistry Registry() { var r=new CoreRegistry(); r.Register(new EmulatorCore{Id="fceumm",DisplayName="FCEUmm",Installed=true,Systems=new[]{GameSystem.Nes}}); r.Register(new EmulatorCore{Id="snes9x",DisplayName="Snes9x",Installed=true,Systems=new[]{GameSystem.Snes}}); return r; }
    [Fact] public void FindsInstalledCoreForNes() { Assert.Equal("fceumm", Assert.Single(Registry().ForSystem(GameSystem.Nes)).Id); }
    [Fact] public void AutoAssignsCompatibleCore() { var g=new GameEntry{Id="mario",RomPath="mario.nes",System=GameSystem.Nes}; Assert.True(new CoreAssignmentService(Registry()).Assign(g).Success); Assert.Equal("fceumm",g.CoreId); }
    [Fact] public void RejectsIncompatiblePreferredCore() { var g=new GameEntry{Id="mario",RomPath="mario.nes",System=GameSystem.Nes}; Assert.False(new CoreAssignmentService(Registry()).Assign(g,"snes9x").Success); }
}
