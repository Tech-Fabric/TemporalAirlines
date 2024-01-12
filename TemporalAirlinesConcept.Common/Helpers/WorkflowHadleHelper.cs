using Temporalio.Client;
using Temporalio.Exceptions;

namespace TemporalAirlinesConcept.Common.Helpers;

public class WorkflowHadleHelper
{
    public static async Task<bool> IsWorkflowExists<T>(ITemporalClient client, string workflowId)
    {
        var handle = client.GetWorkflowHandle<T>(workflowId);

        try
        {
            await handle.DescribeAsync();

            return true;
        }
        catch (RpcException ex)
        {
            if (ex.Code is RpcException.StatusCode.NotFound)
                return false;

            throw;
        }
    }
}
