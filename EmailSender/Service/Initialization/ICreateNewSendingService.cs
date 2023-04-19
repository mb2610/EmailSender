using MacroMail.Models.Dto;

namespace MacroMail.Service.Initialization;

public interface ICreateNewSendingService
{
    Task<Guid> SendAsync(SendingRequest request, CancellationToken cancellationToken);
}