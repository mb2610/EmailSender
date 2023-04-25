namespace MacroMail.Service.Sending;

public interface IRetrievePendingMessageService
{
    public Task RetrievePendingMessageJob(CancellationToken token = default);
}