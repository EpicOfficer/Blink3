namespace Blink3.Common.Extensions;

/// <summary>
///     Provides extension methods for the Task class.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    ///     Forgets the specified <see cref="Task" /> without awaiting its completion and without throwing any exceptions.
    /// </summary>
    /// <param name="task">The <see cref="Task" /> to forget.</param>
    public static void Forget(this Task task)
    {
        if (!task.IsCompleted || task.IsFaulted) _ = ForgetAwaited(task);

        return;

        static async Task ForgetAwaited(Task task)
        {
            await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }
}