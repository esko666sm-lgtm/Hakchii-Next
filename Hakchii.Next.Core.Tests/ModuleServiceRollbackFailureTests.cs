using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModuleServiceRollbackFailureTests
{
    [Fact]
    public async Task RollbackFailure_IsReported()
    {
        var repo = new FakeRepo(new[]
        {
            new ModulePackage { Id = "a", FilePath = "a.hmod" }
        });
        var installer = new FakeInstaller(failInstallId: "root", failUninstallId: "a");
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "a" }
        };

        var result = await new ModuleService(repo, installer).InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("rollback", result.Error, System.StringComparison.OrdinalIgnoreCase);
        Assert.Contains("a", result.Error, System.StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeRepo : IModuleRepository
    {
        private readonly Dictionary<string, ModulePackage> packages;
        public FakeRepo(IEnumerable<ModulePackage> packages)
            => this.packages = packages.ToDictionary(x => x.Id);

        public Task<ModulePackage> GetAsync(string id, CancellationToken cancellationToken)
        {
            packages.TryGetValue(id, out var package);
            return Task.FromResult(package!);
        }
    }

    private sealed class FakeInstaller : IModuleInstaller
    {
        private readonly string? failInstallId;
        private readonly string? failUninstallId;

        public FakeInstaller(string? failInstallId = null, string? failUninstallId = null)
        {
            this.failInstallId = failInstallId;
            this.failUninstallId = failUninstallId;
        }

        public Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken)
            => Task.FromResult(package.Id == failInstallId
                ? OperationResult.Fail("install failed")
                : OperationResult.Ok());

        public Task<OperationResult> UninstallAsync(string moduleId, CancellationToken cancellationToken)
            => Task.FromResult(moduleId == failUninstallId
                ? OperationResult.Fail("uninstall failed")
                : OperationResult.Ok());
    }
}
