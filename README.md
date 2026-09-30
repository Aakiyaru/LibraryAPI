# Library API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)

**RESTful API для управления библиотекой с чистой архитектурой.** Учёт книг, выдача и возврат экземпляров, управление читателями, автоматический расчёт штрафов за просрочку и разграничение прав доступа на основе ролей. Проект построен как портфолио backend-разработчика: показывает владение ASP.NET Core, EF Core, JWT и практиками промышленной разработки.

**RESTful API for library management with clean architecture.** Book inventory, borrowing and returns, reader management, automatic overdue fine calculation, and role-based access control. Built as a backend developer portfolio project demonstrating ASP.NET Core, EF Core, JWT, and production-grade practices.

---

## Содержание

- [Зачем это нужно](#зачем-это-нужно)
- [Как это работает](#как-это-работает)
- [Возможности](#возможности)
- [Стек технологий](#стек-технологий)
- [Архитектура](#архитектура)
- [Структура проекта](#структура-проекта)
- [Требования](#требования)
- [Быстрый старт](#быстрый-старт)
- [Конфигурация](#конфигурация)
- [API Эндпоинты](#api-эндпоинты)
- [Тестирование](#тестирование)
- [CI/CD](#cicd)
- [Устранение неполадок](#устранение-неполадок)
- [Почему именно так](#почему-именно-так)
- [Roadmap](#roadmap)

---

## Зачем это нужно

Библиотека — это не просто «полка с книгами». Это система, где одновременно сосуществуют десятки бизнес-правил:

- одну и ту же книгу в нескольких экземплярах могут читать несколько человек;
- у каждого читателя есть лимит — не больше N книг на руках;
- за просрочку начисляется штраф, но только за **фактически** просроченные дни;
- удаление книги или читателя не должно разрушать историю операций;
- у разных ролей — разные права: админ управляет всем, библиотекарь выдаёт книги, читатель смотрит только себя.

Собрать это всё в одном месте без чёткой архитектуры — прямой путь к запутанному коду, где бизнес-логика размазана по контроллерам и сервисам. Library API демонстрирует, как построить такое приложение **правильно**: с выделенным доменом, инкапсулированными инвариантами, тестируемой логикой и промышленной инфраструктурой.

---

## Как это работает

1. **Клиент** (Swagger, мобильное приложение, SPA) отправляет HTTP-запрос с JWT-токеном в заголовке `Authorization: Bearer <token>`.
2. **Middleware аутентификации** проверяет подпись токена, срок действия и роль пользователя.
3. **Контроллер** принимает запрос, валидирует DTO (через `[ApiController]` + FluentValidation).
4. **Сервис** (`BookService`, `LoanService`, `UserService`) выполняет бизнес-операцию, работая с доменными сущностями.
5. **Доменные сущности** (`Book`, `BookLoan`, `User`) применяют инварианты: нельзя выдать книгу, если нет доступных копий; нельзя удалить пользователя с активными выдачами; штраф считается за каждый начатый день просрочки.
6. **EF Core** сохраняет изменения в SQL Server. Глобальные query filters автоматически исключают soft-deleted записи.
7. **ExceptionHandlingMiddleware** перехватывает исключения и возвращает стандартизированный JSON-ответ.

Схема взаимодействия:

```
 ┌───────────────┐   HTTPS + JWT   ┌──────────────────────┐    EF Core    ┌──────────────┐
 │  Swagger /    │ ──────────────► │   Library.API        │ ────────────► │  SQL Server  │
 │  Мобильное /  │                 │   Controllers        │               │  LibraryDb   │
 │  SPA          │ ◄────────────── │   Middleware         │               └──────────────┘
 └───────────────┘   JSON ответ    └──────────────────────┘
```

---

## Возможности

### Книги

- Полный CRUD: создание, чтение, обновление, удаление
- **Soft Delete** с восстановлением — история не теряется
- **Фильтрация** по жанру и диапазону лет, **поиск** по названию
- **Сортировка** по названию / году (asc / desc)
- **Пагинация** через `page` и `pageSize`
- Уникальность ISBN — на уровне БД и домена
- Защита инварианта: `AvailableCopies` не может превысить `TotalCopies`

### Выдача книг

- Выдача читателю с лимитом **не более 5 активных книг**
- Проверка наличия свободных экземпляров перед выдачей
- Возврат с **автоматическим расчётом штрафа** за просрочку (за каждый начатый день)
- История выдач: активные и завершённые, с фильтром по пользователю
- Защита от двойного возврата одной и той же записи

### Пользователи

- Регистрация и логин через JWT
- **Refresh Tokens** с ротацией (при обновлении старый отзывается)
- Три роли: `Admin`, `Librarian`, `User`
- Управление профилем, безопасная смена пароля
- Soft Delete с проверкой активных выдач перед удалением

### Безопасность

- Хеширование паролей **BCrypt** с автоматической солью
- Access-токены с TTL **15 минут**, refresh-токены — 7 дней
- Ролевая авторизация через `[Authorize(Roles = "...")]`
- Защита от повторного использования refresh-токена (token rotation)
- `ClockSkew = TimeSpan.Zero` — никаких «дефолтных» 5 минут задержки

---

## Стек технологий

| Категория | Технологии |
|---|---|
| Платформа | .NET 9, C# 13 |
| Web-фреймворк | ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| База данных | MS SQL Server (LocalDB / Express / Developer) |
| Аутентификация | JWT Bearer + Refresh Tokens |
| Хеширование | BCrypt.Net-Next |
| Валидация | FluentValidation |
| Документация API | Swagger (Swashbuckle) |
| Тестирование | xUnit, FluentAssertions, Moq |
| CI/CD | GitHub Actions |

---

## Архитектура

Проект построен на принципах **Clean Architecture** (Onion Architecture). Зависимости направлены строго внутрь: внешние слои знают о внутренних, но не наоборот. Это значит, что домен ничего не знает о базе данных, а приложение — о контроллерах.

```
┌──────────────────────────────────────────────┐
│              Library.API (Web API)           │
│  Контроллеры, middleware, конфигурация DI    │
└────────────────────┬─────────────────────────┘
                     │ depends on
┌────────────────────▼─────────────────────────┐
│         Library.Infrastructure               │
│  EF Core, миграции, реализации сервисов      │
└────────────────────┬─────────────────────────┘
                     │ depends on
┌────────────────────▼─────────────────────────┐
│         Library.Application                  │
│  DTO, интерфейсы сервисов, валидаторы        │
└────────────────────┬─────────────────────────┘
                     │ depends on
┌────────────────────▼─────────────────────────┐
│            Library.Domain                    │
│  Сущности, бизнес-правила, инварианты        │
└──────────────────────────────────────────────┘
```

### Слои

1. **Library.Domain** — сущности (`Book`, `User`, `BookLoan`, `RefreshToken`) с инкапсулированной логикой. Никаких зависимостей от других проектов.
2. **Library.Application** — контракты (интерфейсы сервисов), DTO, валидаторы. Зависит только от Domain.
3. **Library.Infrastructure** — реализации: `AppDbContext`, EF-миграции, `BookService`, `AuthService`, `UserService`, `LoanService`. Зависит от Application.
4. **Library.API** — точка входа: контроллеры, middleware, `Program.cs`. Зависит от Infrastructure.

---

## Структура проекта

```
Library/
├── Library.Domain/
│   └── Entities/
│       ├── Book.cs
│       ├── User.cs
│       ├── BookLoan.cs
│       └── RefreshToken.cs
│
├── Library.Application/
│   ├── Dtos/
│   ├── Interfaces/
│   └── Validators/
│
├── Library.Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Migrations/
│   └── Services/
│       ├── BookService.cs
│       ├── UserService.cs
│       ├── LoanService.cs
│       └── AuthService.cs
│
├── Library.API/
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
│
├── Library.Tests/
│   └── Domain/
│       ├── BookTests.cs
│       └── BookLoanTests.cs
│
├── .github/workflows/
│   └── dotnet.yml
└── README.md
```

---

## Требования

| Что | Минимум |
|---|---|
| ОС | Windows 10 / 11, Linux, macOS |
| SDK | [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) |
| БД | SQL Server LocalDB / Express / Developer |
| Инструмент | `dotnet-ef` (`dotnet tool install --global dotnet-ef`) |

> Если SQL Server ставить не хочется — используйте LocalDB, она идёт в комплекте с Visual Studio 2022 или .NET SDK на Windows.

---

## Быстрый старт

### Клонирование

```bash
git clone https://github.com/YOUR_USERNAME/LibraryAPI.git
cd LibraryAPI
```

### Настройка конфигурации

Откройте `Library.API/appsettings.json` и заполните:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "super_secret_key_minimum_32_characters_long_1234567890",
    "Issuer": "LibraryAPI",
    "Audience": "LibraryAPIClient",
    "ExpiryMinutes": 15
  }
}
```

> **Для production** храните `SecretKey` в User Secrets или переменных окружения:
> ```bash
> dotnet user-secrets init --project Library.API
> dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-key" --project Library.API
> ```

### Применение миграций

```bash
dotnet ef database update --project Library.Infrastructure --startup-project Library.API
```

### Запуск

```bash
dotnet run --project Library.API
```

Swagger: **https://localhost:5001/swagger**

### Быстрая проверка

Зарегистрируйте первого пользователя:

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Иван Петров",
    "email": "ivan@example.com",
    "password": "SuperSecret123!"
  }'
```

Ответ будет содержать `accessToken` и `refreshToken`. Нажмите **Authorize** в Swagger и введите `Bearer <accessToken>` — теперь доступны защищённые эндпоинты.

Чтобы получить права администратора, обновите роль в БД:

```sql
USE LibraryDb;
UPDATE Users SET Role = 'Admin' WHERE Email = 'ivan@example.com';
```

Залогиньтесь снова — новый токен будет содержать роль `Admin`.

---

## Конфигурация

Все параметры — в `Library.API/appsettings.json` (и переопределяются через `appsettings.Development.json`, переменные окружения или User Secrets).

| Ключ | Тип | По умолчанию | Описание |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | строка | — | Строка подключения к SQL Server |
| `JwtSettings:SecretKey` | строка | — | Ключ подписи JWT, минимум 32 символа |
| `JwtSettings:Issuer` | строка | `LibraryAPI` | Издатель токена |
| `JwtSettings:Audience` | строка | `LibraryAPIClient` | Получатель токена |
| `JwtSettings:ExpiryMinutes` | int | `15` | Время жизни access-токена в минутах |

### Приоритет источников

1. `appsettings.json` — базовые значения.
2. `appsettings.{Environment}.json` — переопределения для окружения.
3. User Secrets (только в Development).
4. Переменные окружения.
5. Аргументы командной строки.

---

## API Эндпоинты

### Аутентификация

| Метод | Эндпоинт | Описание | Доступ |
|---|---|---|---|
| POST | `/api/auth/register` | Регистрация | Аноним |
| POST | `/api/auth/login` | Вход, выдача токенов | Аноним |
| POST | `/api/auth/refresh` | Обновление access-токена | Аноним (с refresh) |
| POST | `/api/auth/revoke` | Logout (отзыв refresh) | Авторизован |

### Книги

| Метод | Эндпоинт | Описание | Доступ |
|---|---|---|---|
| GET | `/api/books` | Список с фильтрами | Все |
| GET | `/api/books/{id}` | Книга по ID | Все |
| POST | `/api/books` | Создание | Admin, Librarian |
| PUT | `/api/books/{id}` | Обновление | Admin, Librarian |
| DELETE | `/api/books/{id}` | Soft delete | Admin |
| POST | `/api/books/{id}/restore` | Восстановление | Admin |

### Выдачи

| Метод | Эндпоинт | Описание | Доступ |
|---|---|---|---|
| GET | `/api/loans` | Все выдачи | Admin, Librarian |
| GET | `/api/loans/user/{userId}` | Выдачи пользователя | Авторизован |
| POST | `/api/loans/borrow` | Выдать книгу | Авторизован |
| POST | `/api/loans/return` | Вернуть книгу | Авторизован |

### Пользователи

| Метод | Эндпоинт | Описание | Доступ |
|---|---|---|---|
| GET | `/api/users` | Список | Admin, Librarian |
| GET | `/api/users/{id}` | Профиль | Владелец / Staff |
| PUT | `/api/users/{id}` | Обновление | Владелец / Staff |
| POST | `/api/users/{id}/change-password` | Смена пароля | Владелец |
| PATCH | `/api/users/{id}/role` | Смена роли | Admin |
| DELETE | `/api/users/{id}` | Soft delete | Admin |
| POST | `/api/users/{id}/restore` | Восстановление | Admin |

### Примеры запросов

**Список книг с фильтрами, сортировкой и пагинацией:**

```bash
curl "https://localhost:5001/api/books?page=1&pageSize=10&genre=Роман&minYear=1900&sortBy=year&sortDescending=true" \
  -H "Authorization: Bearer <access_token>"
```

**Создание книги (только Admin / Librarian):**

```bash
curl -X POST https://localhost:5001/api/books \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Мастер и Маргарита",
    "isbn": "978-5-04-118636-4",
    "genre": "Фантастика",
    "publicationYear": 1967,
    "totalCopies": 3
  }'
```

**Выдача книги:**

```bash
curl -X POST https://localhost:5001/api/loans/borrow \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{ "userId": "...", "bookId": "...", "loanDays": 14 }'
```

**Возврат с расчётом штрафа:**

```bash
curl -X POST https://localhost:5001/api/loans/return \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{ "loanId": "..." }'
```

В ответе будет `fine` — сумма штрафа (0, если без просрочки; иначе 10₽ × количество начатых дней просрочки).

---

## Тестирование

Запуск всех тестов:

```bash
dotnet test
```

Проект содержит юнит-тесты для бизнес-логики домена — самой важной части приложения, где живут инварианты.

### Что покрыто

**`BookTests`** — 12 тестов:

- Создание с валидными / пустыми данными
- Выдача копии при наличии / отсутствии свободных
- Возврат копии при корректном состоянии / при полном комплекте
- Обновление реквизитов (в т.ч. попытка уменьшить `TotalCopies` ниже выданных)
- Soft delete при активных выдачах / при полном комплекте
- Восстановление после удаления

**`BookLoanTests`** — 4 теста:

- Создание с корректными сроками
- Возврат без просрочки → штраф 0
- Возврат с просрочкой → штраф считается корректно
- Двойной возврат → исключение

Тесты написаны на **xUnit + FluentAssertions**, читаются как спецификация на естественном языке:

```csharp
[Fact]
public void BorrowCopy_WhenNoAvailable_ShouldThrow()
{
    var book = Book.Create("Title", "isbn", "genre", 2000, 1);
    book.BorrowCopy();

    var act = () => book.BorrowCopy();

    act.Should().Throw<InvalidOperationException>()
        .WithMessage("*доступных экземпляров*");
}
```

---

## CI/CD

Пайплайн GitHub Actions (`.github/workflows/dotnet.yml`) запускается на каждый push и pull request в `main`:

1. Checkout репозитория.
2. Установка .NET 9 SDK.
3. `dotnet restore` — восстановление зависимостей.
4. `dotnet build --configuration Release` — сборка.
5. `dotnet test --configuration Release` — прогон всех тестов.

Если сборка или тесты падают — PR блокируется. Это базовая практика, которая показывает, что проект не «сломан на main».

---

## Устранение неполадок

### `AmbiguousMatchException: The request matched multiple endpoints`

У вас два метода с одинаковым `[HttpGet]` в одном контроллере. Удалите устаревший — например, старый `GetAll` рядом с новым `GetBooks`.

### `GH007: Your push would publish a private email address`

GitHub блокирует push, потому что вы используете приватный email в коммитах. Два решения:

1. Включите публичный email: Settings → Emails → снять галку `Keep my email addresses private`.
2. Используйте анонимный email GitHub: `git config --local user.email "123456+username@users.noreply.github.com"`.

Затем перепишите последний коммит:

```bash
git commit --amend --reset-author --no-edit
git push --force-with-lease
```

### `Cannot consume scoped service from singleton`

Вы зарегистрировали сервис, работающий с `AppDbContext`, как `Singleton`. Замените на `Scoped`:

```csharp
builder.Services.AddScoped<IBookService, BookService>();  // ✅
// builder.Services.AddSingleton<IBookService, BookService>();  // ❌
```

### `AddValidatorsFromAssembly` не существует

Метод расширения живёт в отдельном пакете. Установите:

```bash
dotnet add package FluentValidation.DependencyInjection
```

И добавьте `using FluentValidation;`.

### Ошибка `400` при сериализации: `ExecutionContext&`

Это признак того, что где-то потерян `await`. Если в контроллере написано `return Ok(_service.GetAsync())` без `await` — сериализатор пытается сериализовать `Task`, а не результат. Проверьте контроллер.

### `dotnet ef` не находит команду

Установите глобально:

```bash
dotnet tool install --global dotnet-ef
```

Если уже установлено, но версия старая:

```bash
dotnet tool update --global dotnet-ef
```

---

## Почему именно так

Некоторые решения в проекте могут показаться избыточными для учебного портфолио. Ниже — почему они такие.

### Почему Clean Architecture, а не «всё в одном проекте»?

Разделение на четыре слоя даёт три конкретных выигрыша:

- **Тестируемость.** Домен можно тестировать без поднятия БД, DI, HTTP. Все 16 юнит-тестов не касаются инфраструктуры.
- **Заменяемость.** Можно поменять SQL Server на PostgreSQL, добавить gRPC-фасад — без переписывания бизнес-логики.
- **Явные зависимости.** Компилятор сам следит за тем, чтобы домен случайно не начал использовать `DbContext` или `HttpContext`.

### Почему Soft Delete, а не обычный DELETE?

В библиотеке критична история. Если библиотекарь удалит книгу, а через год её попросят в отчёте «сколько всего было экземпляров» — с hard delete данные потеряны. С soft delete книга остаётся в БД с флагом `IsDeleted = true` и глобальным query filter в EF Core автоматически исключается из обычных выборок.

```csharp
modelBuilder.Entity<Book>(entity =>
{
    entity.HasQueryFilter(b => !b.IsDeleted);
});
```

Чтобы получить удалённые явно — `.IgnoreQueryFilters()`.

### Почему Refresh Token Rotation?

Если злоумышленник украдёт refresh-токен, а мы не отзываем старый после использования — он будет иметь доступ к аккаунту до истечения токена (7 дней). Rotation решает эту проблему: при каждом обновлении старый токен отзывается, выдаётся новый. Если старый будет предъявлен повторно — это сигнал о краже, и логика может отозвать всю сессию.

### Почему BCrypt, а не SHA-256?

SHA-256 — быстрая хеш-функция. Она предназначена для проверки целостности данных, а не для хранения паролей. Мощность современных GPU позволяет перебирать миллиарды хешей в секунду. BCrypt специально спроектирован «медленным» и с солью — один пароль занимает ~100 мс на проверку. Это делает брутфорс экономически невыгодным.

### Почему JWT, а не куки с сессией?

JWT хорошо подходит для API, потому что:

- Не требует хранения сессии на сервере (stateless) — легко масштабируется.
- Работает одинаково для мобильных приложений, SPA, серверных интеграций.
- Роли и другие claim'ы передаются прямо в токене, не требуя обращения к БД на каждом запросе.

Плата за это — невозможность мгновенного отзыва access-токена. Именно поэтому access-токен короткий (15 минут), а долгоживущий refresh-токен хранится в БД и может быть отозван.

---

## Roadmap

- [ ] Интеграционные тесты через `WebApplicationFactory`
- [ ] Hangfire — фоновые задачи (напоминания о возврате за день до срока)
- [ ] Email-уведомления: подтверждение регистрации, уведомления о просрочке
- [ ] Health Checks (`/health`) с проверкой БД
- [ ] Rate Limiting — защита от брутфорса логина
- [ ] Полнотекстовый поиск через SQL Server FTS
- [ ] Serilog — структурированное логирование в Seq / ElasticSearch
- [ ] Docker-образ для API + полный `docker-compose`
- [ ] Метрики Prometheus + Grafana
