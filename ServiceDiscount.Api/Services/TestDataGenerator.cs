using ServiceDiscount.Core.Models;
using ServiceDiscount.Core.Services;

namespace ServiceDiscount.Api.Services;

public class TestDataGenerator
{
    public static List<Course> GetTestCourses()
    {
        return new List<Course>
        {
            // Горящая группа (2 дня до старта) — скидка 20%
            new Course { Name = "Python Basics", StartDate = DateTime.Now.AddDays(2), BasePrice = 30000m },
            // Не горящая группа (30 дней) — без скидки
            new Course { Name = "C# Advanced", StartDate = DateTime.Now.AddDays(30), BasePrice = 50000m },
            // Горящая группа (4 дня) — скидка 20%
            new Course { Name = "DevOps Pro", StartDate = DateTime.Now.AddDays(4), BasePrice = 45000m },
            // Не горящая группа (60 дней) — без скидки
            new Course { Name = "Data Science", StartDate = DateTime.Now.AddDays(60), BasePrice = 60000m }
        };
    }

    public static List<UserProfile> GetTestUsers()
    {
        return new List<UserProfile>
        {
            new UserProfile { Name = "Алексей (выпускник)", IsGraduate = true },
            new UserProfile { Name = "Мария (не выпускник)", IsGraduate = false }
        };
    }

    public static Dictionary<string, TestCase> GetTestScenarios()
    {
        var courses = GetTestCourses();
        var users = GetTestUsers();

        return new Dictionary<string, TestCase>
        {
            [
                "Scenario1_HotGroupOnly"
            ] = new TestCase
            {
                Name = "Горящие группы (без скидки выпускника)",
                Courses = courses.Where(c => (c.StartDate - DateTime.Now).Days < 5).ToList(),
                User = users[1] // не выпускник
            },
            [
                "Scenario2_GraduateOnly"
            ] = new TestCase
            {
                Name = "Выпускник (без горящих групп)",
                Courses = courses.Where(c => (c.StartDate - DateTime.Now).Days >= 5).ToList(),
                User = users[0] // выпускник
            },
            [
                "Scenario3_BothFlags"
            ] = new TestCase
            {
                Name = "Горящие группы + выпускник (максимальная скидка)",
                Courses = courses,
                User = users[0] // выпускник
            },
            [
                "Scenario4_NoDiscount"
            ] = new TestCase
            {
                Name = "Нет скидок (не горящие + не выпускник)",
                Courses = courses.Where(c => (c.StartDate - DateTime.Now).Days >= 5).ToList(),
                User = users[1] // не выпускник
            }
        };
    }
}

public class TestCase
{
    public string Name { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = new();
    public UserProfile User { get; set; } = new();
}
