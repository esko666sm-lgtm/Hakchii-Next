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
            var completed = new Stack<TransactionStep>();
            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                OperationResult result;
                try { result = await step.Execute(cancellationToken).ConfigureAwait(false); }
                catch (OperationCanceledException) { await RollbackAsync(completed, CancellationToken.None).ConfigureAwait(false); throw; }
                catch (Exception ex) { result = OperationResult.Fail(step.Name + ": " + ex.Message); }
                if (!result.Success)
                {
                    await RollbackAsync(completed, CancellationToken.None).ConfigureAwait(false);
                    return result;
                }
                completed.Push(step);
            }
            return OperationResult.Ok();
        }

        private static async Task RollbackAsync(Stack<TransactionStep> completed, CancellationToken cancellationToken)
        {
            while (completed.Count > 0)
            {
                var step = completed.Pop();
                if (step.Rollback == null) continue;
                try { await step.Rollback(cancellationToken).ConfigureAwait(false); } catch { }
            }
        }
    }
}
