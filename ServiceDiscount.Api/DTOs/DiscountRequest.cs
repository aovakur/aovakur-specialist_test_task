using ServiceDiscount.Core.Models;

namespace ServiceDiscount.Api.DTOs;

public class DiscountRequest
{
    public required List<CourseDto> Courses { get; set; }
    public UserProfileDto User { get; set; } = new();
}

public class CourseDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public decimal BasePrice { get; set; }
}

public class UserProfileDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsGraduate { get; set; }
}
