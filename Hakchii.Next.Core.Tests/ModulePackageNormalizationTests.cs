using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModulePackageNormalizationTests
{
    [Fact]
    public void Validate_NormalizesIdPathAndDependencies()
    {
        var package = new ModulePackage
        {
            Id = "  retroarch  ",
            FilePath = "  retroarch.hmod  ",
            Dependencies = new[] { "  clovercon  ", " fontfix " }
        };

        var result = new ModulePackageValidator().Validate(package);

        Assert.True(result.Success);
        Assert.Equal("retroarch", package.Id);
        Assert.Equal("retroarch.hmod", package.FilePath);
        Assert.Equal(new[] { "clovercon", "fontfix" }, package.Dependencies);
    }

    [Fact]
    public void Validate_RejectsSelfDependencyAfterNormalization()
    {
        var result = new ModulePackageValidator().Validate(new ModulePackage
        {
            Id = " retroarch ",
            FilePath = "retroarch.hmod",
            Dependencies = new[] { " RETROARCH " }
        });

        Assert.False(result.Success);
    }

    [Fact]
    public void Validate_RejectsDuplicateDependenciesAfterNormalization()
    {
        var result = new ModulePackageValidator().Validate(new ModulePackage
        {
            Id = "retroarch",
            FilePath = "retroarch.hmod",
            Dependencies = new[] { " clovercon ", "CLOVERCON" }
        });

        Assert.False(result.Success);
    }
}
