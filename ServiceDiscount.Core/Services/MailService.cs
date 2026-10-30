using ServiceDiscount.Core.Models;

namespace ServiceDiscount.Core.Services;

public class MailService : IMailService
{
    public Task SendAsync(LeadRequest lead)
    {
        throw new InvalidOperationException("Основной почтовый сервис недоступен. Выполняется переключение на резервный.");
    }
}
