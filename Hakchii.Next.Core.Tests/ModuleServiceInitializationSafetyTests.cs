using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModuleServiceInitializationSafetyTests
{
    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new ModuleService(null!, new Installer()));
        Assert.Throws<ArgumentNullException>(() => new ModuleService(new Repository(), null!));
    }

    [Fact]
    public async Task NullRollbackResult_FailsCleanly()
    {
        var installer = new Installer { FailRoot = true, NullRollback = true };
        var service = new ModuleService(new Repository(), installer);
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "dep" }
        };

        var result = await service.InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("rollback failed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class Repository : IModuleRepository
    {
        public Task<ModulePackage> GetAsync(string id, CancellationToken cancellationToken)
            => Task.FromResult(new ModulePackage { Id = id, FilePath = id + ".hmod" });
    }

    private sealed class Installer : IModuleInstaller
    {
        public bool FailRoot { get; set; }
        public bool NullRollback { get; set; }

        public Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken)
            => Task.FromResult(FailRoot && package.Id == "root"
                ? OperationResult.Fail("root failed")
                : OperationResult.Ok());

        public Task<OperationResult> UninstallAsync(string moduleId, CancellationToken cancellationToken)
            => Task.FromResult(NullRollback ? null! : OperationResult.Ok());
    }
}
