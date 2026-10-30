namespace ServiceDiscount.Api.DTOs;

public class CourseDiscountDetail
{
    public string CourseName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string DiscountReason { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
}

public class DiscountResponse
{
    public decimal TotalPrice { get; set; }
    public List<CourseDiscountDetail> CourseDetails { get; set; } = new();
}
