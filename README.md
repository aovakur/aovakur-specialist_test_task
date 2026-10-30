# ServiceDiscount API

ASP.NET Core Web API — сервис расчёта скидок на курсы и обработки заявок.

## 📋 Техническое задание

### Контекст
В Учебном центре временно недоступны основные каналы связи (технические работы на корпоративных почтовых серверах), а также действует гибкая система скидок на курсы. Необходимо реализовать логику расчёта стоимости со скидками и API-эндпоинт для обработки заявок.

### Технический стек
- **.NET 9.0** (ASP.NET Core Web API)
- **FluentValidation** — валидация входных данных
- **Swashbuckle (Swagger)** — документация API
- **xUnit** — юнит-тесты
- **InMemory** — данные в оперативной памяти (без СУБД/Docker)

---

## Задача 1. Логика расчёта скидок

### Сервис: `DiscountService`

Метод `CalculateTotalAsync(IEnumerable<Course>, UserProfile)` принимает список курсов и статус пользователя, возвращает итоговую стоимость корзины.

### Правила скидок

| Скидка | Условие | Размер |
|--------|---------|--------|
| **Горящая группа** | До старта курса < 5 дней | 20% |
| **Выпускник** | `IsGraduate == true` | 30% |

**Правило бизнеса:** Скидки **не суммируются**. Для каждого курса выбирается **максимальная** из доступных.

---

## Задача 2. Отказоустойчивый контроллер

### Эндпоинт: `POST /api/leads`

Принимает заявку на «Обратный звонок».

#### Входные данные (DTO)

| Поле | Тип | Обязательное | Валидация |
|------|-----|-------------|-----------|
| `name` | string | Да | — |
| `phone` | string | Да | Минимум 10 символов |
| `email` | string | Да | Формат Email |
| `leadType` | enum | Да | `Individual` / `LegalEntity` |

#### Логика

1. **Штатный путь:** Вызов основного почтового сервиса (`IMailService.SendAsync`)
2. **Отказоустойчивость:** При сбое основного сервиса — перехват исключения и отправка на резервный email:
   - Юр. лицо → `specialist.corp@yandex.ru`
   - Физ. лицо → `specialist.ufl@yandex.ru`
3. **Результат:** Клиент всегда получает `200 OK`

---

## API Reference

### Расчёт скидок

#### `POST /api/discounts/calculate`

Расчёт итоговой стоимости корзины курсов со скидками.

**Request:**
```json
{
  "courses": [
    { "name": "Python Basics", "startDate": "2026-11-01T12:00:00", "basePrice": 30000 },
    { "name": "C# Advanced", "startDate": "2026-12-15T12:00:00", "basePrice": 50000 }
  ],
  "user": { "name": "Иван", "isGraduate": true }
}
```

**Response (200 OK):**
```json
{
  "totalPrice": 56000.00,
  "courseDetails": [
    { "courseName": "Python Basics", "basePrice": 30000, "discountPercent": 30, "discountReason": "Выпускник 30%", "finalPrice": 21000 },
    { "courseName": "C# Advanced", "basePrice": 50000, "discountPercent": 30, "discountReason": "Выпускник 30%", "finalPrice": 35000 }
  ]
}
```

#### `GET /api/discounts/test-data`

Возвращает тестовые данные (курсы и пользователи).

#### `POST /api/discounts/test-scenarios`

Запускает тестовые сценарии расчёта скидок.

---

### Обработка заявок

#### `POST /api/leads`

Отправка заявки на обратный звонок.

**Request:**
```json
{
  "name": "Иван Петров",
  "phone": "+79991234567",
  "email": "ivan@example.com",
  "leadType": 0
}
```

**Response (200 OK):**
```json
{ "message": "Заявка успешно обработана" }
```

---

## 🚀 Запуск

### Требования
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)

### Команды

```powershell
# Сборка
dotnet build ServiceDiscount.sln

# Запуск (Swagger: http://localhost:8080/swagger)
dotnet run --project ServiceDiscount.Api

# Публикация (самодостаточный exe)
dotnet publish ServiceDiscount.Api --configuration Release --output publish
.\publish\ServiceDiscount.Api.exe

# Запуск тестов
dotnet test ServiceDiscount.sln
```

---

## 📁 Структура проекта

```
ServiceDiscount/
├── ServiceDiscount.Api/          # ASP.NET Core Web API
│   ├── Controllers/              # LeadsController, DiscountsController
│   ├── DTOs/                     # LeadRequest, DiscountRequest, LeadType
│   ├── Services/                 # TestDataGenerator
│   ├── Program.cs                # Точка входа, DI, Swagger
│   └── appsettings.json
├── ServiceDiscount.Core/         # Бизнес-логика
│   ├── Models/                   # Course, UserProfile
│   └── Services/                 # DiscountService, IMailService
├── ServiceDiscount.Tests/        # xUnit тесты
│   ├── DiscountServiceTests.cs
│   └── LeadsControllerTests.cs
└── ServiceDiscount.sln
```

---

## ✅ Критерии качества

- **Архитектура:** Async/await, разделение на контроллер и сервисный слой, DI
- **Валидация:** FluentValidation (Email, Телефон, обязательные поля)
- **Тесты:** 5 unit-тестов (xUnit) — все проходят

---

## 📝 Архитектура

```
┌─────────────────────────────────────────────────┐
│                  ASP.NET Core API                │
│                                                  │
│  ┌──────────────┐    ┌──────────────────────┐   │
│  │  Controllers  │───>│   DiscountService    │   │
│  │               │    │   IMailService       │   │
│  └──────────────┘    └──────────────────────┘   │
│        │                     │                   │
│        ▼                     ▼                   │
│   Validation           Fallback Logic            │
│   (FluentValidation)   (Reserve Emails)          │
└─────────────────────────────────────────────────┘
```

**Ключевые решения:**
- `IDiscountService` — чистая бизнес-логика, не зависит от HTTP
- `IMailService` с fallback-логикой в контроллере — отказоустойчивость
- FluentValidation — декларативная валидация DTO
- xUnit — покрытие критических сценариев скидок
