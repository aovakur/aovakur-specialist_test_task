using ServiceDiscount.Core.Models;

namespace ServiceDiscount.Core.Services;

public interface IDiscountService
{
    Task<decimal> CalculateTotalAsync(IEnumerable<Course> courses, UserProfile userProfile);
}
