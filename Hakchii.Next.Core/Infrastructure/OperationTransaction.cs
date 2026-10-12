using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Infrastructure
{
    public sealed class TransactionStep
    {
        public string Name { get; set; }
        public Func<CancellationToken, Task<OperationResult>> Execute { get; set; }
        public Func<CancellationToken, Task> Rollback { get; set; }
    }

    public sealed class OperationTransaction
    {
        public async Task<OperationResult> ExecuteAsync(IEnumerable<TransactionStep> steps, CancellationToken cancellationToken)
        {
            if (steps == null)
                return OperationResult.Fail("Transaction steps are required");

            var completed = new Stack<TransactionStep>();

            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (step == null)
                    return await FailWithRollbackAsync(
                        OperationResult.Fail("Transaction step is required"),
                        completed).ConfigureAwait(false);

                if (step.Execute == null)
                    return await FailWithRollbackAsync(
                        OperationResult.Fail((step.Name ?? "Transaction step") + ": execute handler is required"),
                        completed).ConfigureAwait(false);

                OperationResult result;
                try
                {
                    result = await step.Execute(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    await RollbackAsync(completed, CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
                catch (Exception ex)
                {
                    result = OperationResult.Fail((step.Name ?? "Transaction step") + ": " + ex.Message);
                }

                if (!result.Success)
                    return await FailWithRollbackAsync(result, completed).ConfigureAwait(false);

                completed.Push(step);
            }

            return OperationResult.Ok();
        }

        private static async Task<OperationResult> FailWithRollbackAsync(
            OperationResult original,
            Stack<TransactionStep> completed)
        {
            var rollbackErrors = await RollbackAsync(completed, CancellationToken.None).ConfigureAwait(false);
            if (rollbackErrors.Count == 0)
                return original;

            return OperationResult.Fail(
                (original.Error ?? "transaction failed") +
                "; rollback failed: " +
                string.Join(" | ", rollbackErrors));
        }

        private static async Task<List<string>> RollbackAsync(
            Stack<TransactionStep> completed,
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            while (completed.Count > 0)
            {
                var step = completed.Pop();
                if (step.Rollback == null) continue;

                try
                {
                    await step.Rollback(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errors.Add((step.Name ?? "step") + ": " + ex.Message);
                }
            }

            return errors;
        }
    }
}
