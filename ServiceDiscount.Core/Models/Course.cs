namespace ServiceDiscount.Core.Models;

public class Course
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public decimal BasePrice { get; set; }
}
