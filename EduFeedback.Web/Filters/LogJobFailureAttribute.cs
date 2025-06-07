using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.IO;

public class LogJobFailureAttribute : JobFilterAttribute, IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState failedState)
        {
            // Log the failure details
            var logMessage = $"Job {context.BackgroundJob.Id} failed with exception: {failedState.Exception.Message}";
            //File.AppendAllText("hangfire_failures.log", logMessage + Environment.NewLine);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action needed
    }
}
