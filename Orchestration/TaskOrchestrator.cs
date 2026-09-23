namespace AgenticSoftwareEngineering.Orchestration;

public class TaskOrchestrator
{
    private readonly List<AgentTask> _tasks = new();

    public void AddTask(AgentTask task)
    {
        if (!_tasks.Any(t => t.Id == task.Id))
        {
            _tasks.Add(task);
        }
    }

    public List<AgentTask> GetTasks() => _tasks;

    public object GetMetrics() => new
    {
        TotalTasks = _tasks.Count,
        CompletedTasks = _tasks.Count(t => t.Status == TaskStatus.Completed),
        FailedTasks = _tasks.Count(t => t.Status == TaskStatus.Failed)
    };

    public async Task RunWorkflowAsync()
    {
        while (_tasks.Any(t => t.Status != TaskStatus.Completed && t.Status != TaskStatus.RolledBack && t.Status != TaskStatus.Failed))
        {
            var readyTasks = _tasks.Where(t => t.Status == TaskStatus.Pending &&
                t.Dependencies.All(dep => _tasks.First(x => x.Id == dep).Status == TaskStatus.Completed)).ToList();

            if (!readyTasks.Any()) break;

            foreach (var task in readyTasks)
            {
                task.Status = TaskStatus.InProgress;
                await Task.Delay(300); // Simulate execution
                task.Status = TaskStatus.Completed;
            }
        }
    }
}