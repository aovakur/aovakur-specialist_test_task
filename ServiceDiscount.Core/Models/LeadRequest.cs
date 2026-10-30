using System.ComponentModel.DataAnnotations;

namespace ServiceDiscount.Core.Models;

public class LeadRequest
{
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон обязателен для заполнения")]
    [MinLength(10, ErrorMessage = "Телефон должен содержать минимум 10 символов")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Тип клиента обязателен для заполнения")]
    public LeadType LeadType { get; set; }
}
