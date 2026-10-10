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
            if (!string.Equals(Path.GetExtension(package.FilePath), ".hmod", StringComparison.OrdinalIgnoreCase)) return OperationResult.Fail("Module package must use .hmod extension");
            if ((package.Dependencies ?? new string[0]).Any(x => string.IsNullOrWhiteSpace(x))) return OperationResult.Fail("Module dependency id is invalid");
            return OperationResult.Ok();
        }
    }
}
