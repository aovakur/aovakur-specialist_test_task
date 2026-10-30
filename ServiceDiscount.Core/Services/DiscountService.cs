using ServiceDiscount.Core.Models;

namespace ServiceDiscount.Core.Services;

public class DiscountService : IDiscountService
{
    private const int HotGroupDaysThreshold = 5;
    private const decimal HotGroupDiscount = 0.20m;
    private const decimal GraduateDiscount = 0.30m;

    public async Task<decimal> CalculateTotalAsync(IEnumerable<Course> courses, UserProfile userProfile)
    {
        var courseList = courses.ToList();
        decimal total = 0m;

        foreach (var course in courseList)
        {
            decimal discount = CalculateMaxDiscount(course, userProfile);
            decimal discountedPrice = course.BasePrice * (1m - discount);
            total += discountedPrice;
        }

        await Task.CompletedTask;
        return total;
    }

    private decimal CalculateMaxDiscount(Course course, UserProfile userProfile)
    {
        decimal hotGroupDiscount = GetHotGroupDiscount(course);
        decimal graduateDiscount = GetGraduateDiscount(userProfile);

        return Math.Max(hotGroupDiscount, graduateDiscount);
    }

    private decimal GetHotGroupDiscount(Course course)
    {
        var daysUntilStart = (course.StartDate - DateTime.Now).Days;
        return daysUntilStart < HotGroupDaysThreshold ? HotGroupDiscount : 0m;
    }

    private decimal GetGraduateDiscount(UserProfile userProfile)
    {
        return userProfile.IsGraduate ? GraduateDiscount : 0m;
    }
}
