using Temporalio.Client;
using Temporalio.Exceptions;

namespace TemporalAirlinesConcept.Common.Extensions;

public static class TemporalClientExtensions
{
    public static async Task<bool> IsWorkflowRunning<T>(this ITemporalClient client, string workflowId)
    {
        var handle = client.GetWorkflowHandle<T>(workflowId);

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
