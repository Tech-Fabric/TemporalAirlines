using Temporalio.Client;
using Temporalio.Exceptions;

namespace TemporalAirlinesConcept.Common.Helpers;

public class WorkflowHandleHelper
{
    public static async Task<bool> IsWorkflowExists<T>(ITemporalClient client, string workflowId)
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
