using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hakchii.Next.Core.Modules
{
    public sealed class ModulePackageValidator
    {
        public OperationResult Validate(ModulePackage package)
        {
            if (package == null) return OperationResult.Fail("Module package is required");
            if (string.IsNullOrWhiteSpace(package.Id)) return OperationResult.Fail("Module id is required");
            if (string.IsNullOrWhiteSpace(package.FilePath)) return OperationResult.Fail("Module file path is required");

            package.Id = package.Id.Trim();
            package.FilePath = package.FilePath.Trim();

            if (!string.Equals(Path.GetExtension(package.FilePath), ".hmod", StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail("Module package must use .hmod extension");

            var dependencies = (package.Dependencies ?? new string[0])
                .Select(x => x == null ? null : x.Trim())
                .ToArray();
            if (dependencies.Any(x => string.IsNullOrWhiteSpace(x)))
                return OperationResult.Fail("Module dependency id is invalid");

            package.Dependencies = dependencies;

            if (dependencies.Any(x => string.Equals(x, package.Id, StringComparison.OrdinalIgnoreCase)))
                return OperationResult.Fail("Module cannot depend on itself");

            if (dependencies
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1))
                return OperationResult.Fail("Module dependencies contain duplicates");

            return OperationResult.Ok();
        }

        public OperationResult ValidateExisting(ModulePackage package)
        {
            var validation = Validate(package);
            if (!validation.Success) return validation;
            if (!File.Exists(package.FilePath))
                return OperationResult.Fail("Module package file does not exist");
            return OperationResult.Ok();
        }
    }
}
