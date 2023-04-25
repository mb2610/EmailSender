using MacroMail.DbAccess.DataAccess;
using MacroMail.Models;
using MacroMail.Models.Dto;
using MacroMail.Service.Builder;
using MacroMail.Service.Helper;
using MacroMail.Service.Template;

namespace MacroMail.Service.Initialization;

public class CreateNewSendingService : ICreateNewSendingService
{
    private readonly IGroupEmailMessageDataAccess _groupEmailMessageDataAccess;
    private readonly IPendingMessageDataAccess    _pendingMessageDataAccess;
    private readonly IEmailConfigurationService   _emailConfigurationService;
    private readonly IMimeMessageBuilder          _mimeMessageBuilder;
    private readonly ITemplateRenderer            _templateRenderer;

    public CreateNewSendingService(IGroupEmailMessageDataAccess groupEmailMessageDataAccess,
                                   IPendingMessageDataAccess    pendingMessageDataAccess,
                                   IEmailConfigurationService   emailConfigurationService,
                                   IMimeMessageBuilder          mimeMessageBuilder,
                                   ITemplateRenderer            templateRenderer)
    {
        _groupEmailMessageDataAccess = groupEmailMessageDataAccess;
        _emailConfigurationService   = emailConfigurationService;
        _mimeMessageBuilder          = mimeMessageBuilder;
        _templateRenderer            = templateRenderer;
        _pendingMessageDataAccess    = pendingMessageDataAccess;
    }

    public async Task<Guid> SendAsync(SendingRequest request, CancellationToken token)
    {
        var emailConfiguration = await _emailConfigurationService.GetConfigurationAsync(request.Sender);

        var groupSendingUid =
            await _groupEmailMessageDataAccess.CreateAsync(request.Sender, request.Subject, request.Content,
                                                           request.Reply, 1, token);

        var generatedContent = await _templateRenderer.ParseAsync(request.Content, request.Data);
        var emailMessage = new EmailMessage
        {
            Uid     = Guid.NewGuid(),
            Sender  = request.Sender,
            Subject = request.Subject,
            Content = generatedContent,
            To      = request.To.Email,
            Ccs     = request.Ccs.Select(_ => _.Email).ToArray(),
            Reply   = request.Reply
        };
        var mimeMessage = _mimeMessageBuilder.Build(emailMessage, emailConfiguration).Serialize();
        await _pendingMessageDataAccess.CreateAsync(groupSendingUid,
                                                    request.To.Uid,
                                                    request.Ccs.Select(_ => _.Uid).ToArray(),
                                                    mimeMessage,
                                                    token);

        return groupSendingUid;
    }
}