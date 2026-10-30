using FluentAssertions;
using ServiceDiscount.Core.Models;
using ServiceDiscount.Core.Services;

namespace ServiceDiscount.Tests;

public class DiscountServiceTests
{
    [Fact]
    public async Task CalculateTotalAsync_OnlyGraduateDiscount_Applies30Percent()
    {
        // Arrange
        var discountService = new DiscountService();
        var courses = new List<Course>
        {
            new Course { Name = "C# Advanced", StartDate = DateTime.Now.AddDays(30), BasePrice = 50000m },
            new Course { Name = "ASP.NET Core", StartDate = DateTime.Now.AddDays(45), BasePrice = 45000m }
        };
        var userProfile = new UserProfile { Name = "Иван", IsGraduate = true };

        // Act
        var result = await discountService.CalculateTotalAsync(courses, userProfile);

        // Assert
        // 50000 * 0.7 + 45000 * 0.7 = 35000 + 31500 = 66500
        result.Should().Be(66500m);
    }

    [Fact]
    public async Task CalculateTotalAsync_HotGroupAndGraduate_UsesMaxDiscount()
    {
        // Arrange
        var discountService = new DiscountService();
        var courses = new List<Course>
        {
            // Горящая группа (2 дня до старта) + выпускник = скидка 30% (максимальная)
            new Course { Name = "DevOps Basics", StartDate = DateTime.Now.AddDays(2), BasePrice = 40000m },
            // Только выпускник = скидка 30%
            new Course { Name = "Python Basics", StartDate = DateTime.Now.AddDays(60), BasePrice = 30000m }
        };
        var userProfile = new UserProfile { Name = "Мария", IsGraduate = true };

        // Act
        var result = await discountService.CalculateTotalAsync(courses, userProfile);

        // Assert
        // 40000 * 0.7 + 30000 * 0.7 = 28000 + 21000 = 49000
        result.Should().Be(49000m);
    }

    [Fact]
    public async Task CalculateTotalAsync_NoDiscount_NoFlagsApplied()
    {
        // Arrange
        var discountService = new DiscountService();
        var courses = new List<Course>
        {
            new Course { Name = "JavaScript Fundamentals", StartDate = DateTime.Now.AddDays(20), BasePrice = 25000m }
        };
        var userProfile = new UserProfile { Name = "Пётр", IsGraduate = false };

        // Act
        var result = await discountService.CalculateTotalAsync(courses, userProfile);

        // Assert
        result.Should().Be(25000m);
    }
}
