using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Modules
{
    public sealed class ModuleService : IModuleService
    {
        private readonly IModuleRepository repository;
        private readonly IModuleInstaller installer;
        public ModuleService(IModuleRepository repository, IModuleInstaller installer) { this.repository = repository; this.installer = installer; }

        public Task<OperationResult> InstallWithDependenciesAsync(ModulePackage package, CancellationToken cancellationToken)
            => InstallAsync(package, new HashSet<string>(StringComparer.OrdinalIgnoreCase), cancellationToken);

        private async Task<OperationResult> InstallAsync(ModulePackage package, HashSet<string> visiting, CancellationToken cancellationToken)
        {
            if (package == null || string.IsNullOrWhiteSpace(package.Id)) return OperationResult.Fail("Invalid module package");
            if (!visiting.Add(package.Id)) return OperationResult.Fail("Circular module dependency: " + package.Id);
            try
            {
                foreach (var dependencyId in package.Dependencies ?? new string[0])
                {
                    var dependency = await repository.GetAsync(dependencyId, cancellationToken).ConfigureAwait(false);
                    if (dependency == null) return OperationResult.Fail("Missing module dependency: " + dependencyId);
                    var result = await InstallAsync(dependency, visiting, cancellationToken).ConfigureAwait(false);
                    if (!result.Success) return result;
                }
                return await installer.InstallAsync(package, cancellationToken).ConfigureAwait(false);
            }
            finally { visiting.Remove(package.Id); }
        }
    }
}
