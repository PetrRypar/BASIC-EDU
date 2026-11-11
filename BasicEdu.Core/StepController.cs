namespace BasicEdu.Core;

/// <summary>One permit executes one BASIC line; Continue releases all later lines.</summary>
public sealed class StepController(bool stepping)
{
    readonly object sync = new();
    bool continuous = !stepping;
    bool permit = stepping;
    TaskCompletionSource? paused;

    public async Task BeforeLineAsync(int line, Func<int, Task> showPause, CancellationToken token)
    {
        Task wait;
        lock (sync)
        {
            if (continuous) return;
            if (permit) { permit = false; return; }
            paused = new(TaskCreationOptions.RunContinuationsAsynchronously);
            wait = paused.Task;
        }
        await showPause(line);
        await wait.WaitAsync(token);
    }
    public void Step()
    {
        lock (sync)
        {
            if (paused == null) return;
            var release = paused;
            paused = null;
            release.TrySetResult();
        }
    }
    public void Continue()
    {
        lock (sync)
        {
            continuous = true;
            paused?.TrySetResult();
            paused = null;
        }
    }
}
