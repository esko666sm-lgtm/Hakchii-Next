using System;

namespace Hakchii.Next.Core
{
    public sealed class OperationResult
    {
        private OperationResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public bool Success { get; }
        public string Error { get; }
        public static OperationResult Ok() => new OperationResult(true, null);
        public static OperationResult Fail(string error) => new OperationResult(false, error ?? "Unknown error");
    }
}
