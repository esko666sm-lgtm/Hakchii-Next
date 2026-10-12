using System;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Infrastructure;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class OperationTransactionRollbackTests
{
    [Fact]
    public async Task FailedStep_RollsBackCompletedStepsInReverseOrder()
    {
        var order = new System.Collections.Generic.List<string>();
        var tx = new OperationTransaction();

        var result = await tx.ExecuteAsync(new[]
        {
            new TransactionStep
            {
                Name = "one",
                Execute = _ => Task.FromResult(OperationResult.Ok()),
                Rollback = _ => { order.Add("rollback-one"); return Task.CompletedTask; }
            },
            new TransactionStep
            {
                Name = "two",
                Execute = _ => Task.FromResult(OperationResult.Ok()),
                Rollback = _ => { order.Add("rollback-two"); return Task.CompletedTask; }
            },
            new TransactionStep
            {
                Name = "three",
                Execute = _ => Task.FromResult(OperationResult.Fail("boom"))
            }
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(new[] { "rollback-two", "rollback-one" }, order);
    }

    [Fact]
    public async Task RollbackFailure_IsIncludedInError()
    {
        var tx = new OperationTransaction();

        var result = await tx.ExecuteAsync(new[]
        {
            new TransactionStep
            {
                Name = "one",
                Execute = _ => Task.FromResult(OperationResult.Ok()),
                Rollback = _ => throw new InvalidOperationException("undo failed")
            },
            new TransactionStep
            {
                Name = "two",
                Execute = _ => Task.FromResult(OperationResult.Fail("install failed"))
            }
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("install failed", result.Error);
        Assert.Contains("rollback", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("one", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("undo failed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task NullSteps_FailsCleanly()
    {
        var result = await new OperationTransaction().ExecuteAsync(null, CancellationToken.None);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task NullStep_FailsCleanly()
    {
        var result = await new OperationTransaction().ExecuteAsync(
            new TransactionStep[] { null! },
            CancellationToken.None);

        Assert.False(result.Success);
    }
}
