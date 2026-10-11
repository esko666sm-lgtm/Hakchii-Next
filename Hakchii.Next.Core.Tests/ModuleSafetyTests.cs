using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Infrastructure;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;
public class ModuleSafetyTests
{
    [Fact]
    public void ValidatorRejectsNonHmodPackage()
    {
        var result = new ModulePackageValidator().Validate(new ModulePackage { Id = "x", FilePath = "x.zip" });
        Assert.False(result.Success);
    }

    [Fact]
    public void ValidatorAcceptsHmodPackage()
    {
        Assert.True(new ModulePackageValidator().Validate(new ModulePackage { Id = "retroarch", FilePath = "retroarch.hmod" }).Success);
    }

    [Fact]
    public void LocatorReturnsFirstExistingHmodCandidate()
    {
        var result = ModulePackageLocator.Resolve("retroarch", new[] { "a/retroarch.hmod", "b/retroarch.hmod" }, p => p.StartsWith("b/"));
        Assert.True(result.Success);
        Assert.Equal("b/retroarch.hmod", result.Value.FilePath);
    }

    [Fact]
    public void LocatorFailsWhenHmodIsMissing()
    {
        var result = ModulePackageLocator.Resolve("missing", new[] { "a/missing.hmod" }, _ => false);
        Assert.False(result.Success);
        Assert.Contains("not found", result.Error, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TransactionRollsBackCompletedStepsOnFailure()
    {
        var events = new List<string>();
        var tx = new OperationTransaction();
        var result = await tx.ExecuteAsync(new[] {
            new TransactionStep { Name="one", Execute=_ => { events.Add("one"); return Task.FromResult(OperationResult.Ok()); }, Rollback=_ => { events.Add("undo-one"); return Task.CompletedTask; } },
            new TransactionStep { Name="two", Execute=_ => Task.FromResult(OperationResult.Fail("failed")) }
        }, CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal(new[] { "one", "undo-one" }, events);
    }
}
