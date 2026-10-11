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

        public ModuleService(IModuleRepository repository, IModuleInstaller installer)
        {
            this.repository = repository;
            this.installer = installer;
        }

        public Task<OperationResult> InstallWithDependenciesAsync(ModulePackage package, CancellationToken cancellationToken)
            => InstallAsync(
                package,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                cancellationToken);

        private async Task<OperationResult> InstallAsync(
            ModulePackage package,
            HashSet<string> visiting,
            HashSet<string> installed,
            CancellationToken cancellationToken)
        {
            if (package == null || string.IsNullOrWhiteSpace(package.Id))
                return OperationResult.Fail("Invalid module package");

            if (installed.Contains(package.Id))
                return OperationResult.Ok();

            if (!visiting.Add(package.Id))
                return OperationResult.Fail("Circular module dependency: " + package.Id);

            try
            {
                foreach (var dependencyId in package.Dependencies ?? new string[0])
                {
                    if (installed.Contains(dependencyId))
                        continue;

                    var dependency = await repository.GetAsync(dependencyId, cancellationToken).ConfigureAwait(false);
                    if (dependency == null)
                        return OperationResult.Fail("Missing module dependency: " + dependencyId);

                    var dependencyResult = await InstallAsync(dependency, visiting, installed, cancellationToken).ConfigureAwait(false);
                    if (!dependencyResult.Success)
                        return dependencyResult;
                }

                var result = await installer.InstallAsync(package, cancellationToken).ConfigureAwait(false);
                if (result.Success)
                    installed.Add(package.Id);

                return result;
            }
            finally
            {
                visiting.Remove(package.Id);
            }
        }
    }
}
