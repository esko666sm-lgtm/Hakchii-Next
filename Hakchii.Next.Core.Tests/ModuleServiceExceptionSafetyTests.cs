using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Modules;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ModuleServiceExceptionSafetyTests
{
    [Fact]
    public async Task RepositoryException_ReturnsFailureWithoutInstall()
    {
        var installer = new Installer();
        var service = new ModuleService(new ThrowingRepository(), installer);
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "dep" }
        };

        var result = await service.InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("repo failed", result.Error);
        Assert.Empty(installer.Installed);
    }

    [Fact]
    public async Task InstallerException_RollsBackEarlierDependencies()
    {
        var repo = new Repository(new ModulePackage
        {
            Id = "dep",
            FilePath = "dep.hmod"
        });
        var installer = new Installer { ThrowOn = "root" };
        var service = new ModuleService(repo, installer);
        var root = new ModulePackage
        {
            Id = "root",
            FilePath = "root.hmod",
            Dependencies = new[] { "dep" }
        };

        var result = await service.InstallWithDependenciesAsync(root, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("install exploded", result.Error);
        Assert.Contains("dep", installer.Uninstalled);
    }

    private sealed class ThrowingRepository : IModuleRepository
    {
        public Task<ModulePackage> GetAsync(string id, CancellationToken cancellationToken)
            => throw new InvalidOperationException("repo failed");
    }

    private sealed class Repository : IModuleRepository
    {
        private readonly ModulePackage package;
        public Repository(ModulePackage package) { this.package = package; }

        public Task<ModulePackage> GetAsync(string id, CancellationToken cancellationToken)
            => Task.FromResult(package);
    }

    private sealed class Installer : IModuleInstaller
    {
        public string? ThrowOn { get; set; }
        public List<string> Installed { get; } = new List<string>();
        public List<string> Uninstalled { get; } = new List<string>();

        public Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken)
        {
            if (string.Equals(package.Id, ThrowOn, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("install exploded");

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
