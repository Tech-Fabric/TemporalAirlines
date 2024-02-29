using Temporalio.Client;
using Temporalio.Exceptions;

namespace TemporalAirlinesConcept.Common.Extensions;

public static class WorkflowHandleExtensions
{
    public static async Task<bool> IsWorkflowRunning<T>(this WorkflowHandle<T> handle)
    {
        if (handle is null)
            throw new ArgumentNullException();

        try
        {
            var handleDescription = await handle.DescribeAsync();

            var checkResult = handleDescription.Status == Temporalio.Api.Enums.V1.WorkflowExecutionStatus.Running;

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