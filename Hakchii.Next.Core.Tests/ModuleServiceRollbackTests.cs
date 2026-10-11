using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModuleServiceRollbackTests
{
    [Fact]
    public async Task Failure_RollsBackModulesInstalledDuringCurrentRun()
    {
        var repo = new FakeRepo(new[]
        {
            new ModulePackage { Id = "a", FilePath = "a.hmod" },
            new ModulePackage { Id = "b", FilePath = "b.hmod" }
        });
        var installer = new FakeInstaller("root");
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "a", "b" }
        };

        var result = await new ModuleService(repo, installer).InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(new[] { "a", "b" }, installer.Installed);
        Assert.Equal(new[] { "b", "a" }, installer.Uninstalled);
    }

    [Fact]
    public async Task DependencyFailure_RollsBackPreviouslyInstalledDependencies()
    {
        var repo = new FakeRepo(new[]
        {
            new ModulePackage { Id = "a", FilePath = "a.hmod" },
            new ModulePackage { Id = "b", FilePath = "b.hmod" }
        });
        var installer = new FakeInstaller("b");
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "a", "b" }
        };

        var result = await new ModuleService(repo, installer).InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(new[] { "a" }, installer.Installed);
        Assert.Equal(new[] { "a" }, installer.Uninstalled);
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
        private readonly string? failId;
        public List<string> Installed { get; } = new List<string>();
        public List<string> Uninstalled { get; } = new List<string>();

        public FakeInstaller(string? failId = null) { this.failId = failId; }

        public Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken)
        {
            if (package.Id == failId)
                return Task.FromResult(OperationResult.Fail("install failed"));

            Installed.Add(package.Id);
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> UninstallAsync(string moduleId, CancellationToken cancellationToken)
        {
            Uninstalled.Add(moduleId);
            return Task.FromResult(OperationResult.Ok());
        }
    }
}
