using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;
public class ModuleServiceTests
{
    [Fact]
    public async Task InstallsDependencyBeforeRequestedModule()
    {
        var dep = new ModulePackage { Id = "retroarch" };
        var main = new ModulePackage { Id = "core-fceumm", Dependencies = new[] { "retroarch" } };
        var installer = new Installer();
        var service = new ModuleService(new Repo(dep), installer);
        var result = await service.InstallWithDependenciesAsync(main, CancellationToken.None);
        Assert.True(result.Success);
        Assert.Equal(new[] { "retroarch", "core-fceumm" }, installer.Installed);
    }

    [Fact]
    public async Task MissingDependencyStopsInstall()
    {
        var service = new ModuleService(new Repo(), new Installer());
        var result = await service.InstallWithDependenciesAsync(new ModulePackage { Id = "core", Dependencies = new[] { "missing" } }, CancellationToken.None);
        Assert.False(result.Success);
        Assert.Contains("Missing", result.Error);
    }

    private sealed class Repo : IModuleRepository
    {
        private readonly Dictionary<string, ModulePackage> items = new Dictionary<string, ModulePackage>();
        public Repo(params ModulePackage[] modules) { foreach (var m in modules) items[m.Id] = m; }
        public Task<ModulePackage> GetAsync(string id, CancellationToken cancellationToken) { items.TryGetValue(id, out var m); return Task.FromResult(m!); }
    }
    private sealed class Installer : IModuleInstaller
    {
        public List<string> Installed { get; } = new List<string>();
        public Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken) { Installed.Add(package.Id); return Task.FromResult(OperationResult.Ok()); }
        public Task<OperationResult> UninstallAsync(string moduleId, CancellationToken cancellationToken) => Task.FromResult(OperationResult.Ok());
    }
}

