using System;
using System.IO;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModulePackageValidationTests
{
    [Fact]
    public void ValidateExisting_RejectsMissingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".hmod");
        var result = new ModulePackageValidator().ValidateExisting(new ModulePackage { Id = "x", FilePath = path });
        Assert.False(result.Success);
    }

    [Fact]
    public void ValidateExisting_AcceptsExistingHmod()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".hmod");
        File.WriteAllText(path, "x");
        try
        {
            var result = new ModulePackageValidator().ValidateExisting(new ModulePackage { Id = "x", FilePath = path });
            Assert.True(result.Success);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Validate_RejectsSelfDependency()
    {
        var result = new ModulePackageValidator().Validate(new ModulePackage
        {
            Id = "retroarch",
            FilePath = "retroarch.hmod",
            Dependencies = new[] { "retroarch" }
        });
        Assert.False(result.Success);
    }

    [Fact]
    public void Validate_RejectsDuplicateDependencies()
    {
        var result = new ModulePackageValidator().Validate(new ModulePackage
        {
            Id = "retroarch",
            FilePath = "retroarch.hmod",
            Dependencies = new[] { "clovercon", "CLOVERCON" }
        });
        Assert.False(result.Success);
    }

    [Fact]
    public void Locator_SkipsExistingNonHmodCandidate()
    {
        var result = ModulePackageLocator.Resolve(
            "retroarch",
            new[] { "a/retroarch.zip", "b/retroarch.hmod" },
            _ => true);

        Assert.True(result.Success);
        Assert.Equal("b/retroarch.hmod", result.Value.FilePath);
    }
}
