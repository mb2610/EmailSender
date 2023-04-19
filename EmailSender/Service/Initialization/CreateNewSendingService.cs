using Hangfire;
using Hangfire.States;
using MacroMail.DbAccess.DataAccess;
using MacroMail.Models;
using MacroMail.Models.Dto;
using MacroMail.Service.Builder;
using MacroMail.Service.Helper;
using MacroMail.Service.Sending;
using MacroMail.Service.Template;

namespace MacroMail.Service.Initialization;

public class CreateNewSendingService : ICreateNewSendingService
{
    private readonly IGroupEmailMessageDataAccess _groupEmailMessageDataAccess;
    private readonly ITrackingMessageDataAccess   _trackingMessageDataAccess;
    private readonly IEmailConfigurationService   _emailConfigurationService;
    private readonly IMimeMessageBuilder          _mimeMessageBuilder;
    private readonly ITemplateRenderer            _templateRenderer;
    private readonly IBackgroundJobClient         _backgroundJobClient;

    public CreateNewSendingService(IGroupEmailMessageDataAccess groupEmailMessageDataAccess,
                                   ITrackingMessageDataAccess   trackingMessageDataAccess,
                                   IEmailConfigurationService   emailConfigurationService,
                                   IMimeMessageBuilder          mimeMessageBuilder,
                                   ITemplateRenderer            templateRenderer,
                                   IBackgroundJobClient         backgroundJobClient)
    {
        _groupEmailMessageDataAccess = groupEmailMessageDataAccess;
        _emailConfigurationService   = emailConfigurationService;
        _mimeMessageBuilder          = mimeMessageBuilder;
        _templateRenderer            = templateRenderer;
        _backgroundJobClient         = backgroundJobClient;
        _trackingMessageDataAccess   = trackingMessageDataAccess;
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
        await _trackingMessageDataAccess.CreateAsync(emailMessage.Uid, groupSendingUid, request.To.Uid,
                                                     request.Ccs.Select(_ => _.Uid).ToArray(),
                                                     mimeMessage, token);

        var queue = new EnqueuedState("myQueueName");
        _backgroundJobClient.Create<ISendingService>(job => job.SendAsync(emailMessage.Uid, CancellationToken.None), queue);
        
        return groupSendingUid;
    }
}