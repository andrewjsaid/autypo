using System.Diagnostics;

namespace Autypo;

/// <summary>
/// Ensures that only one reload operation runs at a time, while allowing others to await its completion.
/// </summary>
/// <remarks>
/// This gate is used to prevent concurrent executions of a reload routine such as <c>ReloadAsync</c>
/// while also providing an option to retry if a reload was already in progress.
/// </remarks>
internal class AsyncReloadGate
{
    private Task? _task;

    /// <summary>
    /// Triggers the provided <paramref name="completer"/> to reload, ensuring only one concurrent execution.
    /// </summary>
    /// <param name="completer">The <see cref="AutypoSearch"/> instance whose reload logic will be invoked.</param>
    /// <param name="triggerAgainIfAlreadyRunning">
    /// If <c>true</c>, and a reload is already running, this method will wait and re-trigger the reload once the ongoing one completes.
    /// If <c>false</c>, this method will simply await the current reload without re-invoking it.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting or reloading.</param>
    /// <returns>A task that completes when the reload (or chained reloads) has finished.</returns>
    /// <remarks>
    /// Multiple callers may invoke this method concurrently. If a reload is already underway, other callers will await
    /// its completion. The first caller enters the critical section and triggers the actual reload logic.
    /// </remarks>
    public async Task TriggerAsync(AutypoSearch completer, bool triggerAgainIfAlreadyRunning, CancellationToken cancellationToken)
    {
        bool tryAgain;

        do
        {
            tryAgain = false;

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var currentTask = Interlocked.CompareExchange(ref _task, tcs.Task, null);
            bool lockTaken = currentTask is null;

            if (!lockTaken)
            {
                await currentTask!.WaitAsync(cancellationToken);

                if (triggerAgainIfAlreadyRunning)
                {
                    triggerAgainIfAlreadyRunning = false;
                    tryAgain = true;
                }
            }
            else
            {
                try
                {
                    await completer.ReloadAsync(cancellationToken);

                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    throw;
                }
                finally
                {
                    var lockingTask = Interlocked.Exchange(ref _task, null);
                    Debug.Assert(lockingTask == tcs.Task);
                }
            }
        } while (tryAgain);
    }
}

