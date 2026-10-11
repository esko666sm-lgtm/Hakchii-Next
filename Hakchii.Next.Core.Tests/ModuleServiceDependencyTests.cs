using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModuleServiceDependencyTests
{
    [Fact]
    public async Task SharedDependency_IsInstalledOnlyOnce()
    {
        var repo = new FakeRepo(new[]
        {
            new ModulePackage { Id = "shared", FilePath = "shared.hmod" },
            new ModulePackage { Id = "left", FilePath = "left.hmod", Dependencies = new[] { "shared" } },
            new ModulePackage { Id = "right", FilePath = "right.hmod", Dependencies = new[] { "shared" } }
        });
        var installer = new FakeInstaller();
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "left", "right" }
        };

        var result = await new ModuleService(repo, installer).InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, installer.Installed.Count(x => x == "shared"));
        Assert.Equal(new[] { "shared", "left", "right", "root" }, installer.Installed);
    }

    [Fact]
    public async Task DependencyFailure_StopsParentInstall()
    {
        var repo = new FakeRepo(new[]
        {
            new ModulePackage { Id = "dep", FilePath = "dep.hmod" }
        });
        var installer = new FakeInstaller("dep");
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "dep" }
        };

        var result = await new ModuleService(repo, installer).InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.DoesNotContain("root", installer.Installed);
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

        public FakeInstaller(string? failId = null) { this.failId = failId; }

        public Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken)
        {
            if (package.Id == failId)
                return Task.FromResult(OperationResult.Fail("install failed"));

            Installed.Add(package.Id);
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> UninstallAsync(string moduleId, CancellationToken cancellationToken)
            => Task.FromResult(OperationResult.Ok());
    }
}
