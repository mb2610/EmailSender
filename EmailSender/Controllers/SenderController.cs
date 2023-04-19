using MacroMail.Models.Dto;
using MacroMail.Service.Initialization;
using MacroMail.Service.Sending;
using Microsoft.AspNetCore.Mvc;

namespace MacroMail.Controllers;

[ApiController]
[Route("/v1/[controller]s/")]
public class SenderController : ControllerBase
{
    private readonly ICreateNewSendingService  _createNewSendingService;
    private readonly ILogger<SenderController> _logger;

    public SenderController(ICreateNewSendingService createNewSendingService, ILogger<SenderController> logger)
    {
        _createNewSendingService = createNewSendingService;
        _logger                  = logger;
    }

    [HttpPost]
    public async Task<ActionResult<SendingResponse>> SendSingleEmailAsync(
        [FromBody] SendingRequest request, CancellationToken token)
    {
        await _createNewSendingService.SendAsync(request, token);
        return Ok();
    }

    [HttpGet("{sendingUid:guid}")]
    public ActionResult<SendingResponse> Get(Guid sendingUid) { return Ok(); }
}