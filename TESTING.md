# Инструкция по тестированию

## Запуск проекта

```powershell
# Перейти в папку проекта
cd ServiceDiscount.Api

# Запустить приложение
dotnet run
```

После запуска API будет доступен по адресу `http://127.0.0.1:5000` (или другому порту, выводится в консоли).

---

## Задача 1: Расчёт скидок

### 1.1 GET /api/discounts/test-data — посмотреть тестовые данные

```powershell
curl -X GET "http://127.0.0.1:5000/api/discounts/test-data"
```

**Ожидаемый результат:** Список тестовых курсов (с пометкой `isHotGroup`) и тестовых пользователей.

---

### 1.2 POST /api/discounts/calculate — расчёт по произвольным данным

```powershell
# Пример: выпускник + горящая группа
curl -X POST "http://127.0.0.1:5000/api/discounts/calculate" `
  -H "Content-Type: application/json" `
  -d '{
    "courses": [
      {"name": "Python Basics", "startDate": "' + (Get-Date).AddDays(2).ToString("yyyy-MM-dd") + '", "basePrice": 30000},
      {"name": "C# Advanced", "startDate": "' + (Get-Date).AddDays(30).ToString("yyyy-MM-dd") + '", "basePrice": 50000}
    ],
    "user": {"name": "Алексей", "isGraduate": true}
  }'
```

**Ожидаемый результат:** `200 OK` с детализацией по каждому курсу и итоговой суммой.

---

### 1.3 POST /api/discounts/test-scenarios — запуск всех тестовых сценариев

```powershell
curl -X POST "http://127.0.0.1:5000/api/discounts/test-scenarios"
```

**Ожидаемый результат:** `200 OK` с результатами 4 тестовых сценариев:

| # | Сценарий | Ожидаемая логика |
|---|----------|-----------------|
| 1 | Горящие группы (без выпускника) | Скидка 20% на горящие курсы |
| 2 | Выпускник (без горящих групп) | Скидка 30% на все курсы |
| 3 | Горящие группы + выпускник | Максимальная скидка (30%), не суммируются |
| 4 | Нет скидок | Без скидок |

---

## Задача 2: Отправка заявки (POST /api/leads)

### 1. POST /api/leads — отправка заявки (штатный сценарий)

```powershell
# Юр. лицо
curl -X POST "http://127.0.0.1:5000/api/leads" `
  -H "Content-Type: application/json" `
  -d '{"name":"Иван Петров","phone":"+79001234567","email":"ivan@corp.ru","leadType":1}'

# Физ. лицо
curl -X POST "http://127.0.0.1:5000/api/leads" `
  -H "Content-Type: application/json" `
  -d '{"name":"Мария Сидорова","phone":"+79161234567","email":"maria@mail.ru","leadType":0}'
```

**Ожидаемый результат:** `200 OK` — заявка обрабатывается успешно (даже при сбое основного почтового сервиса, срабатывает fallback).

---

### 2. POST /api/leads — невалидные данные

```powershell
# Некорректный email
curl -X POST "http://127.0.0.1:5000/api/leads" `
  -H "Content-Type: application/json" `
  -d '{"name":"Иван","phone":"+79001234567","email":"invalid-email","leadType":0}'

# Пустое имя и короткий телефон
curl -X POST "http://127.0.0.1:5000/api/leads" `
  -H "Content-Type: application/json" `
  -d '{"name":"","phone":"123","email":"test@test.com","leadType":1}'

# Пропущен обязательный телефон
curl -X POST "http://127.0.0.1:5000/api/leads" `
  -H "Content-Type: application/json" `
  -d '{"name":"Иван","email":"test@test.com","leadType":0}'
```

**Ожидаемый результат:** `400 Bad Request` — в ответе список ошибок валидации.

---

## Запуск Unit-тестов

```powershell
# Запустить все тесты
dotnet test

# Запустить тесты с подробным выводом
dotnet test --verbosity normal

# Запустить только тесты на DiscountService
dotnet test --filter "FullyQualifiedName~DiscountServiceTests"

# Запустить только тесты на LeadsController
dotnet test --filter "FullyQualifiedName~LeadsControllerTests"
```

### Какие тесты написаны:

| # | Тест | Что проверяет |
|---|------|---------------|
| 1 | `CalculateTotalAsync_OnlyGraduateDiscount_Applies30Percent` | Скидка 30% для выпускника |
| 2 | `CalculateTotalAsync_HotGroupAndGraduate_UsesMaxDiscount` | Выбирается максимальная скидка (30%), а не суммируются |
| 3 | `CalculateTotalAsync_NoDiscount_NoFlagsApplied` | Нет скидки, если ни один флаг не активен |
| 4 | `PostLead_FallbackMailOnFailure_ReturnsOk` | Контроллер возвращает 200 при сбое почтового сервиса |
| 5 | `PostLead_InvalidModel_ReturnsBadRequest` | Валидация возвращает 400 при невалидных данных |

---

## Проверка через Postman / Swagger

1. Запустите проект: `dotnet run`
2. Откройте в браузере: `http://127.0.0.1:5000/swagger`
3. Используйте Swagger UI для тестирования эндпоинта `/api/leads`

---

## Примеры JSON для копирования

```json
{
  "name": "Алексей Иванов",
  "phone": "+79031234567",
  "email": "alexey@example.com",
  "leadType": 1
}
```

```json
{
  "name": "Ольга Смирнова",
  "phone": "+79169876543",
  "email": "olga@gmail.com",
  "leadType": 0
}
```

---

## Примеры JSON для расчёта скидок

### Сценарий 1: Только горящие группы (не выпускник)

```json
{
  "courses": [
    {"name": "Python Basics", "startDate": "2026-10-31", "basePrice": 30000},
    {"name": "DevOps Pro", "startDate": "2026-11-02", "basePrice": 45000}
  ],
  "user": {"name": "Мария", "isGraduate": false}
}
```

**Ожидаемый результат:**
- Python Basics: 30000 * 0.8 = 24000 (скидка 20% — горящая группа)
- DevOps Pro: 45000 * 0.8 = 36000 (скидка 20% — горящая группа)
- **Итого: 60000**

---

### Сценарий 2: Только выпускник (не горящие группы)

```json
{
  "courses": [
    {"name": "C# Advanced", "startDate": "2026-11-28", "basePrice": 50000},
    {"name": "Data Science", "startDate": "2026-12-28", "basePrice": 60000}
  ],
  "user": {"name": "Алексей", "isGraduate": true}
}
```

**Ожидаемый результат:**
- C# Advanced: 50000 * 0.7 = 35000 (скидка 30% — выпускник)
- Data Science: 60000 * 0.7 = 42000 (скидка 30% — выпускник)
- **Итого: 77000**

---

### Сценарий 3: Горящие группы + выпускник (максимальная скидка)

```json
{
  "courses": [
    {"name": "Python Basics", "startDate": "2026-10-31", "basePrice": 30000},
    {"name": "C# Advanced", "startDate": "2026-11-28", "basePrice": 50000}
  ],
  "user": {"name": "Алексей", "isGraduate": true}
}
```

**Ожидаемый результат:**
- Python Basics: 30000 * 0.7 = 21000 (скидка 30% — выпускник > горящая группа 20%)
- C# Advanced: 50000 * 0.7 = 35000 (скидка 30% — выпускник)
- **Итого: 56000** (скидки НЕ суммируются!)

---

### Сценарий 4: Нет скидок

```json
{
  "courses": [
    {"name": "C# Advanced", "startDate": "2026-11-28", "basePrice": 50000},
    {"name": "Data Science", "startDate": "2026-12-28", "basePrice": 60000}
  ],
  "user": {"name": "Мария", "isGraduate": false}
}
```

**Ожидаемый результат:**
- C# Advanced: 50000 (без скидки)
- Data Science: 60000 (без скидки)
- **Итого: 110000**
