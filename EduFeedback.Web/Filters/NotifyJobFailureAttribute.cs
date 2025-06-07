using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System.Net;
using System.Net.Mail;

public class NotifyJobFailureAttribute : JobFilterAttribute, IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState failedState)
        {
            // Send notification
            var subject = $"Job {context.BackgroundJob.Id} failed";
            var body = $"Job {context.BackgroundJob.Id} failed with exception: {failedState.Exception.Message}";

            SendEmailNotification(subject, body);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action needed
    }

    private void SendEmailNotification(string subject, string body)
    {
        var smtpClient = new SmtpClient("smtp.example.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("username", "password"),
            EnableSsl = true,
        };

        //smtpClient.Send("from@example.com", "to@example.com", subject, body);
    }
}
