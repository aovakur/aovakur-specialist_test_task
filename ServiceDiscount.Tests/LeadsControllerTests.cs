using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceDiscount.Api.Controllers;
using ServiceDiscount.Core.Models;
using ServiceDiscount.Core.Services;

namespace ServiceDiscount.Tests;

public class LeadsControllerTests
{
    [Fact]
    public async Task PostLead_FallbackMailOnFailure_ReturnsOk()
    {
        // Arrange
        var mailServiceMock = new Mock<IMailService>();
        mailServiceMock
            .Setup(s => s.SendAsync(It.IsAny<LeadRequest>()))
            .Throws(new InvalidOperationException("Основной почтовый сервис недоступен"));

        var loggerMock = new Mock<ILogger<LeadsController>>();
        var controller = new LeadsController(mailServiceMock.Object, loggerMock.Object);

        var request = new LeadRequest
        {
            Name = "Алексей",
            Phone = "+79001234567",
            Email = "alexey@test.com",
            LeadType = LeadType.LegalEntity
        };

        // Act
        var result = await controller.Post(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task PostLead_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var mailServiceMock = new Mock<IMailService>();
        var loggerMock = new Mock<ILogger<LeadsController>>();
        var controller = new LeadsController(mailServiceMock.Object, loggerMock.Object);

        var request = new LeadRequest
        {
            Name = "",
            Phone = "123",
            Email = "invalid-email",
            LeadType = LeadType.Individual
        };

        controller.ModelState.AddModelError("Name", "Имя обязательно");
        controller.ModelState.AddModelError("Phone", "Телефон должен содержать минимум 10 символов");
        controller.ModelState.AddModelError("Email", "Некорректный формат Email");

        // Act
        var result = await controller.Post(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
