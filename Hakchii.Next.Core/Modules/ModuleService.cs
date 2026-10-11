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

        public async Task<OperationResult> InstallWithDependenciesAsync(ModulePackage package, CancellationToken cancellationToken)
        {
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var installedThisRun = new List<string>();

            var result = await InstallAsync(package, visiting, installed, installedThisRun, cancellationToken).ConfigureAwait(false);
            if (result.Success)
                return result;

            var rollbackErrors = new List<string>();
            for (var i = installedThisRun.Count - 1; i >= 0; i--)
            {
                var moduleId = installedThisRun[i];
                try
                {
                    var rollbackResult = await installer.UninstallAsync(moduleId, cancellationToken).ConfigureAwait(false);
                    if (!rollbackResult.Success)
                        rollbackErrors.Add(moduleId + ": " + (rollbackResult.Error ?? "rollback failed"));
                }
                catch (Exception ex)
                {
                    rollbackErrors.Add(moduleId + ": " + ex.Message);
                }
            }

            if (rollbackErrors.Count > 0)
                return OperationResult.Fail((result.Error ?? "install failed") + "; rollback failed: " + string.Join(" | ", rollbackErrors));

            return result;
        }

        private async Task<OperationResult> InstallAsync(
            ModulePackage package,
            HashSet<string> visiting,
            HashSet<string> installed,
            List<string> installedThisRun,
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

                    var dependencyResult = await InstallAsync(
                        dependency,
                        visiting,
                        installed,
                        installedThisRun,
                        cancellationToken).ConfigureAwait(false);

                    if (!dependencyResult.Success)
                        return dependencyResult;
                }

                var result = await installer.InstallAsync(package, cancellationToken).ConfigureAwait(false);
                if (result.Success)
                {
                    installed.Add(package.Id);
                    installedThisRun.Add(package.Id);
                }

                return result;
            }
            finally
            {
                visiting.Remove(package.Id);
            }
        }
    }
}
