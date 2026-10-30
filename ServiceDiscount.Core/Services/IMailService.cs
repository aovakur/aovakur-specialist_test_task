using ServiceDiscount.Core.Models;

namespace ServiceDiscount.Core.Services;

public interface IMailService
{
    Task SendAsync(LeadRequest lead);
}
