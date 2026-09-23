namespace AgenticSoftwareEngineering.Orchestration;

public enum TaskStatus { Pending, InProgress, RequiresHumanApproval, Completed, Failed, RolledBack }

public class AgentTask
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public bool RequiresApproval { get; set; } = false;
    public int RetryCount { get; set; } = 0;
}
