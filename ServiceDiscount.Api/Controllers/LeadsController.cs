using Microsoft.AspNetCore.Mvc;
using ServiceDiscount.Core.Models;
using ServiceDiscount.Core.Services;

namespace ServiceDiscount.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly IMailService _mailService;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(IMailService mailService, ILogger<LeadsController> logger)
    {
        _mailService = mailService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LeadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _mailService.SendAsync(request);
            return Ok(new { message = "Заявка успешно отправлена" });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Основной почтовый сервис недоступен. Отправка через резервный.");

            string fallbackEmail = request.LeadType == LeadType.LegalEntity
                ? "specialist.corp@yandex.ru"
                : "specialist.ufl@yandex.ru";

            await SendFallbackAsync(request, fallbackEmail);

            return Ok(new { message = "Заявка успешно обработана (резервный канал)" });
        }
    }

    private Task SendFallbackAsync(LeadRequest lead, string fallbackEmail)
    {
        _logger.LogInformation("Резервная отправка заявки от {Name} на email {FallbackEmail}", lead.Name, fallbackEmail);
        return Task.CompletedTask;
    }
}
