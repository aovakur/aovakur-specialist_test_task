using Microsoft.AspNetCore.Mvc;
using ServiceDiscount.Api.DTOs;
using ServiceDiscount.Api.Services;
using ServiceDiscount.Core.Models;
using ServiceDiscount.Core.Services;

namespace ServiceDiscount.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<DiscountsController> _logger;

    public DiscountsController(IDiscountService discountService, ILogger<DiscountsController> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    /// <summary>
    /// Расчёт итоговой стоимости корзины курсов со скидками
    /// </summary>
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] DiscountRequest request)
    {
        if (request.Courses == null || !request.Courses.Any())
        {
            return BadRequest(new { error = "Список курсов не может быть пустым" });
        }

        // Преобразование DTO в модели
        var courses = request.Courses.Select(c => new Course
        {
            Name = c.Name,
            StartDate = c.StartDate,
            BasePrice = c.BasePrice
        }).ToList();

        var userProfile = new UserProfile
        {
            Name = request.User.Name,
            IsGraduate = request.User.IsGraduate
        };

        var totalPrice = await _discountService.CalculateTotalAsync(courses, userProfile);

        var details = request.Courses.Select(c =>
        {
            var daysUntilStart = (c.StartDate - DateTime.Now).Days;
            var hotGroupDiscount = daysUntilStart < 5 ? 0.20m : 0m;
            var graduateDiscount = request.User.IsGraduate ? 0.30m : 0m;
            var maxDiscount = Math.Max(hotGroupDiscount, graduateDiscount);

            string reason = maxDiscount == 0.30m
                ? (hotGroupDiscount == 0.30m ? "Горящая группа + Выпускник (максимум 30%)" : "Выпускник 30%")
                : maxDiscount == 0.20m
                    ? "Горящая группа 20%"
                    : "Нет скидок";

            return new CourseDiscountDetail
            {
                CourseName = c.Name,
                BasePrice = c.BasePrice,
                DiscountPercent = maxDiscount * 100,
                DiscountReason = reason,
                FinalPrice = c.BasePrice * (1m - maxDiscount)
            };
        }).ToList();

        var response = new DiscountResponse
        {
            TotalPrice = totalPrice,
            CourseDetails = details
        };

        return Ok(response);
    }

    /// <summary>
    /// Получение тестовых данных (курсы и пользователи)
    /// </summary>
    [HttpGet("test-data")]
    public IActionResult GetTestData()
    {
        var courses = TestDataGenerator.GetTestCourses();
        var users = TestDataGenerator.GetTestUsers();

        var response = new
        {
            courses = courses.Select(c => new
            {
                c.Name,
                c.StartDate,
                c.BasePrice,
                DaysUntilStart = (c.StartDate - DateTime.Now).Days,
                IsHotGroup = (c.StartDate - DateTime.Now).Days < 5
            }),
            users = users.Select(u => new
            {
                u.Name,
                u.IsGraduate
            })
        };

        return Ok(response);
    }

    /// <summary>
    /// Запуск тестовых сценариев расчёта скидок
    /// </summary>
    [HttpPost("test-scenarios")]
    public async Task<IActionResult> RunTestScenarios()
    {
        var scenarios = TestDataGenerator.GetTestScenarios();
        var results = new List<ScenarioResult>();

        foreach (var kvp in scenarios)
        {
            var scenario = kvp.Value;
            var totalPrice = await _discountService.CalculateTotalAsync(scenario.Courses, scenario.User);

            var details = scenario.Courses.Select(c =>
            {
                var daysUntilStart = (c.StartDate - DateTime.Now).Days;
                var hotGroupDiscount = daysUntilStart < 5 ? 0.20m : 0m;
                var graduateDiscount = scenario.User.IsGraduate ? 0.30m : 0m;
                var maxDiscount = Math.Max(hotGroupDiscount, graduateDiscount);

                string reason = maxDiscount == 0.30m
                    ? (hotGroupDiscount == 0.30m ? "Горящая группа + Выпускник (максимум 30%)" : "Выпускник 30%")
                    : maxDiscount == 0.20m
                        ? "Горящая группа 20%"
                        : "Нет скидок";

                return new CourseDetailResult
                {
                    CourseName = c.Name,
                    BasePrice = c.BasePrice,
                    DiscountPercent = maxDiscount * 100,
                    DiscountReason = reason,
                    FinalPrice = c.BasePrice * (1m - maxDiscount)
                };
            }).ToList();

            results.Add(new ScenarioResult
            {
                ScenarioName = scenario.Name,
                TotalPrice = totalPrice,
                CourseDetails = details
            });
        }

        return Ok(new { testScenarios = results });
    }
}

public class ScenarioResult
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public List<CourseDetailResult> CourseDetails { get; set; } = new();
}

public class CourseDetailResult
{
    public string CourseName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string DiscountReason { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
}
