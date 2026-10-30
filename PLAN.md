# План реализации задачи «ServiceDiscount»

## 1. Создание проекта

- [ ] Инициализация решения (`.sln`) и ASP.NET Core Web API проекта
- [ ] Настройка `.csproj`: целевая версия .NET 8, NuGet-пакеты (xUnit, Moq, FluentValidation)
- [ ] Создание структуры папок:

```
ServiceDiscount/
├── src/
│   ├── ServiceDiscount.Api/          # Контроллеры, DTO, Program.cs
│   └── ServiceDiscount.Core/         # Сервисы, доменная логика
└── tests/
    └── ServiceDiscount.Tests/        # xUnit-тесты
```

---

## 2. Доменная модель и DTO

- [ ] `Course` — модель курса (Название, СтартоваяДата, БазоваяСтоимость)
- [ ] `UserProfile` — профиль пользователя (Имя, IsGraduate)
- [ ] `LeadRequest` — DTO входящей заявки (Имя, Телефон, Email, ТипКлиента)
- [ ] `LeadType` — перечисление: `Individual` / `LegalEntity`

---

## 3. Задача 1 — Сервис расчёта скидок

- [ ] Интерфейс `IDiscountService` с методом `CalculateTotalAsync(IEnumerable<Course>, UserProfile)`
- [ ] Реализация `DiscountService`:
  - [ ] Проверка «горящей группы»: `DateTime.Now - Course.StartDate < 5 дней → скидка 20%`
  - [ ] Проверка «выпускника»: `UserProfile.IsGraduate == true → скидка 30%`
  - [ ] Бизнес-правило: **не суммировать** — выбирать максимальную скидку для каждого курса
  - [ ] Возврат итоговой стоимости корзины
- [ ] Асинхронный метод (`async`/`await`), хотя логика синхронная — для соответствия архитектуре

---

## 4. Задача 2 — Отказоустойчивый контроллер

- [ ] Интерфейс `IMailService` с методом `SendAsync(LeadRequest)`
- [ ] Реализация `MailService` — имитация отправки (бросает `InvalidOperationException` для симуляции сбоя)
- [ ] Контроллер `LeadsController`:
  - [ ] `POST /api/leads`
  - [ ] Валидация входящих данных (FluentValidation или `[Required]` + `[EmailAddress]`)
  - [ ] `try/catch`: при сбое основного почтового сервиса — отправка на резервный email
  - [ ] Логика резервного email:
    - `LegalEntity` → `specialist.corp@yandex.ru`
    - `Individual` → `specialist.ufl@yandex.ru`
  - [ ] Возврат `200 OK` в любом случае

---

## 5. Dependency Injection

- [ ] Регистрация сервисов в `Program.cs`:
  - `builder.Services.AddScoped<IDiscountService, DiscountService>();`
  - `builder.Services.AddScoped<IMailService, MailService>();`
  - `builder.Services.AddControllers();`
  - `builder.Services.AddEndpointsApiExplorer();`
  - `builder.Services.AddSwaggerGen();`

---

## 6. Валидация

- [ ] Email-адрес: валидный формат (`[EmailAddress]`)
- [ ] Телефон: заполнен, не пустой (`[Required]`, минимум 10 символов)
- [ ] Имя: заполнено (`[Required]`)
- [ ] Тип клиента: задан (`[Required]`)

---

## 7. Unit-тесты (xUnit)

- [ ] **Тест 1:** `CalculateTotalAsync_OnlyGraduateDiscount` — IsGraduate=true, скидка 30%
- [ ] **Тест 2:** `CalculateTotalAsync_UseMaxDiscount` — горящая группа + выпускник, выбирается 30%
- [ ] **Тест 3:** `PostLead_FallbackMailOnFailure` — MailService бросает исключение, контроллер отправляет на резервный email

---

## 8. Финальная сборка и проверка

- [ ] `dotnet build` — без ошибок
- [ ] `dotnet test` — все тесты зелёные
- [ ] Swagger UI доступен по `/swagger`
- [ ] Код выложен в публичный репозиторий (GitHub / GitLab)

---

## Оценка трудозатрат

| Этап | Ориентировочно |
|------|---------------|
| Создание проекта + структура | 30 мин |
| Доменная модель + DTO | 30 мин |
| Сервис скидок | 1–1.5 ч |
| Контроллер + отказоустойчивость | 1–1.5 ч |
| Тесты | 1–1.5 ч |
| Сборка, проверка, деплой в репо | 30 мин |
| **Итого** | **~4–6 часов** |
