using System;
using System.Collections.Generic;
using System.Linq;

namespace Hakchii.Next.Core.Modules
{
    public sealed class ModulePackageLookupResult
    {
        private ModulePackageLookupResult(bool success, ModulePackage value, string error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public bool Success { get; }
        public ModulePackage Value { get; }
        public string Error { get; }

        public static ModulePackageLookupResult Ok(ModulePackage package) => new ModulePackageLookupResult(true, package, null);
        public static ModulePackageLookupResult Fail(string error) => new ModulePackageLookupResult(false, null, error);
    }

    public static class ModulePackageLocator
    {
        public static ModulePackageLookupResult Resolve(string moduleId, IEnumerable<string> candidates, Func<string, bool> exists)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                return ModulePackageLookupResult.Fail("Module id is required");
            if (candidates == null || exists == null)
                return ModulePackageLookupResult.Fail("Module candidates are required");

            var validator = new ModulePackageValidator();
            foreach (var path in candidates.Where(exists))
            {
                var package = new ModulePackage { Id = moduleId, FilePath = path };
                var validation = validator.Validate(package);
                if (validation.Success)
                    return ModulePackageLookupResult.Ok(package);
            }

            return ModulePackageLookupResult.Fail("Module package not found: " + moduleId);
        }
    }
}
