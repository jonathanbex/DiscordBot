using System.Collections.Concurrent;

namespace BotApplication.Queue
{
	public class TaskQueue
	{
		private readonly ConcurrentQueue<Func<Task>> _taskQueue = new ConcurrentQueue<Func<Task>>();
		private readonly SemaphoreSlim _semaphore;
		private readonly List<Task> _runningTasks = new List<Task>();

		public TaskQueue()
		{
			_semaphore = new SemaphoreSlim(10);
		}

		public void Enqueue(Func<Task> task)
		{
			_taskQueue.Enqueue(async () =>
			{
				await _semaphore.WaitAsync();
				try
				{
					await task();
				}
				finally
				{
					_semaphore.Release();
				}
			});

			// Process tasks concurrently
			_ = ProcessQueueAsync();
		}

		private async Task ProcessQueueAsync()
		{
			while (_taskQueue.TryDequeue(out var task))
			{
				_runningTasks.Add(Task.Run(task));
			}

			// Wait for a set of tasks to complete
			if (_runningTasks.Count >= _semaphore.CurrentCount) // Adjust the count if needed
			{
				var completedTask = await Task.WhenAny(_runningTasks);
				_runningTasks.Remove(completedTask);
			}
		}
	}
}
