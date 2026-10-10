using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Modules
{
    public sealed class ModulePackage
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string FilePath { get; set; }
        public IReadOnlyList<string> Dependencies { get; set; } = new string[0];
    }

    public interface IModuleRepository
    {
        Task<ModulePackage> GetAsync(string id, CancellationToken cancellationToken);
    }

    public interface IModuleInstaller
    {
        Task<OperationResult> InstallAsync(ModulePackage package, CancellationToken cancellationToken);
        Task<OperationResult> UninstallAsync(string moduleId, CancellationToken cancellationToken);
    }

    public interface IModuleService
    {
        Task<OperationResult> InstallWithDependenciesAsync(ModulePackage package, CancellationToken cancellationToken);
    }
}
