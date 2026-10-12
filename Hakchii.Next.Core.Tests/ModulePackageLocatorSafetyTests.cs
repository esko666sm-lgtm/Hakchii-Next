using System.Collections.Generic;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModulePackageLocatorSafetyTests
{
    [Fact]
    public void Resolve_TrimsModuleIdAndCandidatePath()
    {
        var result = ModulePackageLocator.Resolve(
            "  retroarch  ",
            new[] { "  mods/retroarch.hmod  " },
            path => path == "mods/retroarch.hmod");

        Assert.True(result.Success);
        Assert.Equal("retroarch", result.Value.Id);
        Assert.Equal("mods/retroarch.hmod", result.Value.FilePath);
    }

    [Fact]
    public void Resolve_SkipsBlankCandidatesWithoutCallingExists()
    {
        var seen = new List<string>();

        var result = ModulePackageLocator.Resolve(
            "retroarch",
            new string[] { null!, "", "   ", "mods/retroarch.hmod" },
            path =>
            {
                seen.Add(path);
                return path == "mods/retroarch.hmod";
            });

        Assert.True(result.Success);
        Assert.Single(seen);
        Assert.Equal("mods/retroarch.hmod", seen[0]);
    }

    [Fact]
    public void Resolve_DeduplicatesNormalizedCandidates()
    {
        var seen = new List<string>();

        var result = ModulePackageLocator.Resolve(
            "retroarch",
            new[] { " mods/retroarch.hmod ", "MODS/RETROARCH.HMOD" },
            path =>
            {
                seen.Add(path);
                return false;
            });

        Assert.False(result.Success);
        Assert.Single(seen);
    }
}
