using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace TemporalAirlinesConcept.Common.Extensions;

public static class WorkflowHandleExtensions
{
    public static async Task<bool> IsWorkflowRunning<T>(this WorkflowHandle<T> handle)
    {
        var isRunning = await handle.IsWorkflowInStatuses([WorkflowExecutionStatus.Running]);

        return isRunning;
    }

    public static async Task<bool> IsWorkflowRunningOrCompleted<T>(this WorkflowHandle<T> handle)
    {
        var isRunning = await handle
            .IsWorkflowInStatuses([WorkflowExecutionStatus.Running, WorkflowExecutionStatus.Completed]);

        return isRunning;
    }

    public static async Task<bool> IsWorkflowInStatuses<T>(this WorkflowHandle<T> handle,
        params WorkflowExecutionStatus[] statuses)
    {
        if (handle is null)
            throw new ArgumentNullException();

        if (statuses is null || !statuses.Any())
            throw new ArgumentNullException();

        try
        {
            var handleDescription = await handle.DescribeAsync();

            var checkResult = statuses.Any(x => x == handleDescription.Status);

            return checkResult;
        }
        catch (RpcException ex)
        {
            if (ex.Code is RpcException.StatusCode.NotFound)
                return false;

            throw;
        }
    }
}