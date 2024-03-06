using Temporalio.Client;

namespace TemporalAirlinesConcept.Common.Extensions;

public static class TemporalClientExtensions
{
    public static async Task<bool> IsWorkflowRunning<T>(this ITemporalClient client, string workflowId)
    {
        var handle = client.GetWorkflowHandle<T>(workflowId);

        var isRunning = await handle.IsWorkflowRunning();

        return isRunning;
    }
    
    public static async Task<bool> IsWorkflowRunningOrCompleted<T>(this ITemporalClient client, string workflowId)
    {
        var handle = client.GetWorkflowHandle<T>(workflowId);

        var isRunning = await handle.IsWorkflowRunningOrCompleted();

        return isRunning;
    }
}